using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.BeneficiaryServices;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.BeneficiaryServices;

public class BeneficiaryServicesService(ApplicationDbcontext dbcontext) : IBeneficiaryServicesService
{
    public async Task<Result<BeneficiaryServicesDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var aidRequestsCount = await dbcontext.BeneficiaryAidRequests.CountAsync(cancellationToken);
        var pendingAidRequestsCount = await dbcontext.BeneficiaryAidRequests.CountAsync(x => x.Status < AidRequestStatus.RejectedWithNote, cancellationToken);
        var paymentOrdersCount = await dbcontext.BeneficiaryPaymentOrders.CountAsync(cancellationToken);
        var sponsorshipRecordsCount = await dbcontext.SponsorshipRecords.CountAsync(cancellationToken);
        var dueSponsorshipPaymentsCount = await dbcontext.SponsorshipPayments.CountAsync(x => x.Status == SponsorshipPaymentStatus.Pending && x.DueDate <= DateTime.UtcNow.AddHours(3), cancellationToken);
        var entitySupportRequestsCount = await dbcontext.EntitySupportRequests.CountAsync(cancellationToken);
        var couponRequestsCount = await dbcontext.CouponRequests.CountAsync(cancellationToken);
        var approvedAidAmount = await dbcontext.BeneficiaryAidRequests
            .Where(x => x.Status == AidRequestStatus.CommitteeApproved || x.Status == AidRequestStatus.ManagerApproved)
            .SumAsync(x => x.Amount, cancellationToken);
        var paymentOrdersAmount = await dbcontext.BeneficiaryPaymentOrders
            .Where(x => x.Status == PaymentOrderStatus.Approved || x.Status == PaymentOrderStatus.Closed)
            .SumAsync(x => x.Amount, cancellationToken);

        return Result.Success(new BeneficiaryServicesDashboardResponse(
            aidRequestsCount,
            pendingAidRequestsCount,
            paymentOrdersCount,
            sponsorshipRecordsCount,
            dueSponsorshipPaymentsCount,
            entitySupportRequestsCount,
            couponRequestsCount,
            approvedAidAmount,
            paymentOrdersAmount));
    }

    public async Task<Result<IEnumerable<AidRequestResponse>>> GetAidRequestsAsync(AidRequestStatus? status, bool? isExternal, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryAidRequests.AsNoTracking().Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryEntity).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (isExternal.HasValue)
            query = query.Where(x => x.IsExternal == isExternal.Value);

        var requests = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapAidRequest(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<AidRequestResponse>>(requests);
    }

    public async Task<Result<AidRequestResponse>> SaveAidRequestAsync(int? id, SaveAidRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AidType) || string.IsNullOrWhiteSpace(request.Description) || request.Amount < 0)
            return Result.Failure<AidRequestResponse>(BeneficiaryServicesErrors.InvalidRequest);

        if (request.BeneficiaryProfileId.HasValue && !await dbcontext.BeneficiaryProfiles.AnyAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken))
            return Result.Failure<AidRequestResponse>(BeneficiaryErrors.ProfileNotFound);
        if (request.BeneficiaryEntityId.HasValue && !await dbcontext.BeneficiaryEntities.AnyAsync(x => x.Id == request.BeneficiaryEntityId.Value, cancellationToken))
            return Result.Failure<AidRequestResponse>(BeneficiaryErrors.EntityNotFound);

        var requestNumber = string.IsNullOrWhiteSpace(request.RequestNumber)
            ? await GenerateAidRequestNumberAsync(cancellationToken)
            : request.RequestNumber.Trim();
        var duplicate = await dbcontext.BeneficiaryAidRequests.AnyAsync(x => x.RequestNumber == requestNumber && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicate)
            return Result.Failure<AidRequestResponse>(BeneficiaryServicesErrors.DuplicateAidRequestNumber);

        BeneficiaryAidRequest aidRequest;
        if (id.HasValue)
        {
            aidRequest = await dbcontext.BeneficiaryAidRequests.Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryEntity).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (aidRequest is null)
                return Result.Failure<AidRequestResponse>(BeneficiaryServicesErrors.AidRequestNotFound);
        }
        else
        {
            aidRequest = new BeneficiaryAidRequest();
            dbcontext.BeneficiaryAidRequests.Add(aidRequest);
        }

        aidRequest.BeneficiaryProfileId = request.BeneficiaryProfileId;
        aidRequest.BeneficiaryEntityId = request.BeneficiaryEntityId;
        aidRequest.RequestNumber = requestNumber;
        aidRequest.AidType = request.AidType.Trim();
        aidRequest.Amount = request.Amount;
        aidRequest.Description = request.Description.Trim();
        aidRequest.IsExternal = request.IsExternal;
        if (request.IsExternal && aidRequest.Status == AidRequestStatus.Draft)
            aidRequest.Status = AidRequestStatus.External;

        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadAidRequestReferencesAsync(aidRequest, cancellationToken);
        return Result.Success(MapAidRequest(aidRequest));
    }

    public async Task<Result<AidRequestResponse>> DecideAidRequestAsync(int id, DecideAidRequestRequest request, CancellationToken cancellationToken = default)
    {
        var aidRequest = await dbcontext.BeneficiaryAidRequests.Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryEntity).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aidRequest is null)
            return Result.Failure<AidRequestResponse>(BeneficiaryServicesErrors.AidRequestNotFound);

        aidRequest.Status = request.Status;
        aidRequest.SocialResearchNotes = request.SocialResearchNotes?.Trim();
        aidRequest.DecisionNotes = request.DecisionNotes?.Trim();
        aidRequest.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAidRequest(aidRequest));
    }

    public async Task<Result<PaymentOrderResponse>> CreatePaymentOrderFromAidRequestAsync(int aidRequestId, CreatePaymentOrderFromAidRequestRequest request, CancellationToken cancellationToken = default)
    {
        var aidRequest = await dbcontext.BeneficiaryAidRequests
            .Include(x => x.BeneficiaryProfile)
            .Include(x => x.BeneficiaryEntity)
            .FirstOrDefaultAsync(x => x.Id == aidRequestId, cancellationToken);

        if (aidRequest is null)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.AidRequestNotFound);

        if (aidRequest.Status is not (AidRequestStatus.ManagerApproved or AidRequestStatus.CommitteeApproved or AidRequestStatus.Transferred))
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.AidRequestNotApproved);

        var existingOrder = await dbcontext.BeneficiaryPaymentOrders
            .AnyAsync(x => x.BeneficiaryAidRequestId == aidRequest.Id && x.Status != PaymentOrderStatus.Removed, cancellationToken);
        if (existingOrder)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.DuplicatePaymentOrderForAidRequest);

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? aidRequest.DecisionNotes
            : request.Notes.Trim();
        var order = new BeneficiaryPaymentOrder
        {
            BeneficiaryAidRequestId = aidRequest.Id,
            BeneficiaryAidRequest = aidRequest,
            BeneficiaryProfileId = aidRequest.BeneficiaryProfileId,
            BeneficiaryProfile = aidRequest.BeneficiaryProfile,
            OrderNumber = await GeneratePaymentOrderNumberAsync(cancellationToken),
            OrderType = request.OrderType,
            Amount = aidRequest.Amount,
            ItemDescription = $"{aidRequest.AidType} - {aidRequest.Description}",
            DueDate = request.DueDate?.Date,
            DecisionNotes = notes
        };

        aidRequest.Status = AidRequestStatus.Transferred;
        aidRequest.DecidedAt = DateTime.UtcNow.AddHours(3);
        dbcontext.BeneficiaryPaymentOrders.Add(order);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPaymentOrder(order));
    }

    public async Task<Result<PaymentOrderResponse>> CreatePaymentOrderFromEntitySupportAsync(int entitySupportId, CreatePaymentOrderFromEntitySupportRequest request, CancellationToken cancellationToken = default)
    {
        var support = await dbcontext.EntitySupportRequests
            .Include(x => x.BeneficiaryEntity)
            .FirstOrDefaultAsync(x => x.Id == entitySupportId, cancellationToken);

        if (support is null)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.EntitySupportNotFound);
        if (support.Status != EntitySupportStatus.Approved)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.EntitySupportNotApproved);

        var existingOrder = await dbcontext.BeneficiaryPaymentOrders
            .AnyAsync(x => x.EntitySupportRequestId == support.Id && x.Status != PaymentOrderStatus.Removed, cancellationToken);
        if (existingOrder)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.DuplicatePaymentOrderForEntitySupport);

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? support.DecisionNotes
            : request.Notes.Trim();
        var order = new BeneficiaryPaymentOrder
        {
            EntitySupportRequestId = support.Id,
            EntitySupportRequest = support,
            OrderNumber = await GeneratePaymentOrderNumberAsync(cancellationToken),
            OrderType = request.OrderType,
            Amount = support.Amount,
            ItemDescription = $"{support.SupportType} - {support.RequesterName}",
            DueDate = request.DueDate?.Date,
            DecisionNotes = notes
        };

        dbcontext.BeneficiaryPaymentOrders.Add(order);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPaymentOrder(order));
    }

    public async Task<Result<IEnumerable<PaymentOrderResponse>>> GetPaymentOrdersAsync(PaymentOrderType? type, PaymentOrderStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.BeneficiaryPaymentOrders.AsNoTracking().Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryAidRequest).Include(x => x.EntitySupportRequest).AsQueryable();
        if (type.HasValue)
            query = query.Where(x => x.OrderType == type.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var orders = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapPaymentOrder(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<PaymentOrderResponse>>(orders);
    }

    public async Task<Result<PaymentOrderResponse>> SavePaymentOrderAsync(int? id, SavePaymentOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.ItemDescription))
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.InvalidRequest);

        var aidRequest = request.BeneficiaryAidRequestId.HasValue
            ? await dbcontext.BeneficiaryAidRequests.FirstOrDefaultAsync(x => x.Id == request.BeneficiaryAidRequestId.Value, cancellationToken)
            : null;
        if (request.BeneficiaryAidRequestId.HasValue && aidRequest is null)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.AidRequestNotFound);
        if (request.BeneficiaryProfileId.HasValue && !await dbcontext.BeneficiaryProfiles.AnyAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken))
            return Result.Failure<PaymentOrderResponse>(BeneficiaryErrors.ProfileNotFound);

        var orderNumber = string.IsNullOrWhiteSpace(request.OrderNumber)
            ? await GeneratePaymentOrderNumberAsync(cancellationToken)
            : request.OrderNumber.Trim();
        var duplicate = await dbcontext.BeneficiaryPaymentOrders.AnyAsync(x => x.OrderNumber == orderNumber && (!id.HasValue || x.Id != id.Value), cancellationToken);
        if (duplicate)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.DuplicatePaymentOrderNumber);

        BeneficiaryPaymentOrder order;
        if (id.HasValue)
        {
            order = await dbcontext.BeneficiaryPaymentOrders.Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryAidRequest).Include(x => x.EntitySupportRequest).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (order is null)
                return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.PaymentOrderNotFound);
        }
        else
        {
            order = new BeneficiaryPaymentOrder();
            dbcontext.BeneficiaryPaymentOrders.Add(order);
        }

        order.BeneficiaryAidRequestId = request.BeneficiaryAidRequestId;
        order.BeneficiaryProfileId = request.BeneficiaryProfileId ?? aidRequest?.BeneficiaryProfileId;
        order.OrderNumber = orderNumber;
        order.OrderType = request.OrderType;
        order.Amount = request.Amount;
        order.ItemDescription = request.ItemDescription.Trim();
        order.DueDate = request.DueDate?.Date;

        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadPaymentOrderReferencesAsync(order, cancellationToken);
        return Result.Success(MapPaymentOrder(order));
    }

    public async Task<Result<PaymentOrderResponse>> DecidePaymentOrderAsync(int id, DecidePaymentOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await dbcontext.BeneficiaryPaymentOrders.Include(x => x.BeneficiaryProfile).Include(x => x.BeneficiaryAidRequest).Include(x => x.EntitySupportRequest).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
            return Result.Failure<PaymentOrderResponse>(BeneficiaryServicesErrors.PaymentOrderNotFound);

        order.Status = request.Status;
        order.DecisionNotes = request.DecisionNotes?.Trim();
        order.ClosedAt = request.Status == PaymentOrderStatus.Closed ? DateTime.UtcNow.AddHours(3) : order.ClosedAt;
        if (request.Status == PaymentOrderStatus.Closed && order.BeneficiaryAidRequest is not null)
        {
            order.BeneficiaryAidRequest.Status = AidRequestStatus.Closed;
            order.BeneficiaryAidRequest.DecisionNotes = request.DecisionNotes?.Trim() ?? order.BeneficiaryAidRequest.DecisionNotes;
            order.BeneficiaryAidRequest.DecidedAt = order.ClosedAt;
        }
        if (request.Status == PaymentOrderStatus.Closed && order.EntitySupportRequest is not null)
        {
            order.EntitySupportRequest.Status = EntitySupportStatus.Closed;
            order.EntitySupportRequest.DecisionNotes = request.DecisionNotes?.Trim() ?? order.EntitySupportRequest.DecisionNotes;
        }
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPaymentOrder(order));
    }

    public async Task<Result<IEnumerable<SponsorResponse>>> GetSponsorsAsync(SponsorStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.Sponsors.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var sponsors = await query.OrderBy(x => x.FullName).Select(x => MapSponsor(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<SponsorResponse>>(sponsors);
    }

    public async Task<Result<SponsorResponse>> SaveSponsorAsync(int? id, SaveSponsorRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<SponsorResponse>(BeneficiaryServicesErrors.InvalidRequest);

        Sponsor sponsor;
        if (id.HasValue)
        {
            sponsor = await dbcontext.Sponsors.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (sponsor is null)
                return Result.Failure<SponsorResponse>(BeneficiaryServicesErrors.SponsorNotFound);
        }
        else
        {
            sponsor = new Sponsor();
            dbcontext.Sponsors.Add(sponsor);
        }

        sponsor.FullName = request.FullName.Trim();
        sponsor.Mobile = request.Mobile?.Trim();
        sponsor.Email = request.Email?.Trim();
        sponsor.Status = request.Status;
        sponsor.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSponsor(sponsor));
    }

    public async Task<Result<IEnumerable<SponsorshipRequirementResponse>>> GetSponsorshipRequirementsAsync(SponsorshipStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SponsorshipRequirements.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var requirements = await query.OrderBy(x => x.Title).Select(x => MapRequirement(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<SponsorshipRequirementResponse>>(requirements);
    }

    public async Task<Result<SponsorshipRequirementResponse>> SaveSponsorshipRequirementAsync(int? id, SaveSponsorshipRequirementRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Amount <= 0)
            return Result.Failure<SponsorshipRequirementResponse>(BeneficiaryServicesErrors.InvalidRequest);

        SponsorshipRequirement requirement;
        if (id.HasValue)
        {
            requirement = await dbcontext.SponsorshipRequirements.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (requirement is null)
                return Result.Failure<SponsorshipRequirementResponse>(BeneficiaryServicesErrors.SponsorshipRequirementNotFound);
        }
        else
        {
            requirement = new SponsorshipRequirement();
            dbcontext.SponsorshipRequirements.Add(requirement);
        }

        requirement.Title = request.Title.Trim();
        requirement.Amount = request.Amount;
        requirement.Frequency = string.IsNullOrWhiteSpace(request.Frequency) ? "Monthly" : request.Frequency.Trim();
        requirement.Status = request.Status;
        requirement.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRequirement(requirement));
    }

    public async Task<Result<IEnumerable<SponsorshipRecordResponse>>> GetSponsorshipRecordsAsync(SponsorshipStatus? status, CancellationToken cancellationToken = default)
    {
        var query = SponsorshipRecordQuery();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var records = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapSponsorshipRecord(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<SponsorshipRecordResponse>>(records);
    }

    public async Task<Result<SponsorshipRecordResponse>> SaveSponsorshipRecordAsync(int? id, SaveSponsorshipRecordRequest request, CancellationToken cancellationToken = default)
    {
        var sponsor = await dbcontext.Sponsors.FirstOrDefaultAsync(x => x.Id == request.SponsorId, cancellationToken);
        if (sponsor is null)
            return Result.Failure<SponsorshipRecordResponse>(BeneficiaryServicesErrors.SponsorNotFound);
        if (request.BeneficiaryProfileId.HasValue && !await dbcontext.BeneficiaryProfiles.AnyAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken))
            return Result.Failure<SponsorshipRecordResponse>(BeneficiaryErrors.ProfileNotFound);
        if (request.SponsorshipRequirementId.HasValue && !await dbcontext.SponsorshipRequirements.AnyAsync(x => x.Id == request.SponsorshipRequirementId.Value, cancellationToken))
            return Result.Failure<SponsorshipRecordResponse>(BeneficiaryServicesErrors.SponsorshipRequirementNotFound);
        if (request.Amount <= 0 || (request.EndsAt.HasValue && request.EndsAt.Value.Date < request.StartsAt.Date))
            return Result.Failure<SponsorshipRecordResponse>(BeneficiaryServicesErrors.InvalidRequest);

        SponsorshipRecord record;
        if (id.HasValue)
        {
            record = await SponsorshipRecordQuery(false).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (record is null)
                return Result.Failure<SponsorshipRecordResponse>(BeneficiaryServicesErrors.SponsorshipRecordNotFound);
        }
        else
        {
            record = new SponsorshipRecord { Sponsor = sponsor };
            dbcontext.SponsorshipRecords.Add(record);
        }

        record.SponsorId = request.SponsorId;
        record.BeneficiaryProfileId = request.BeneficiaryProfileId;
        record.SponsorshipRequirementId = request.SponsorshipRequirementId;
        record.StartsAt = request.StartsAt.Date;
        record.EndsAt = request.EndsAt?.Date;
        record.Amount = request.Amount;
        record.Status = request.Status;
        record.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await LoadSponsorshipRecordReferencesAsync(record, cancellationToken);
        return Result.Success(MapSponsorshipRecord(record));
    }

    public async Task<Result<IEnumerable<SponsorshipPaymentResponse>>> GetSponsorshipPaymentsAsync(SponsorshipPaymentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.SponsorshipPayments
            .AsNoTracking()
            .Include(x => x.SponsorshipRecord)
            .ThenInclude(x => x!.Sponsor)
            .Include(x => x.SponsorshipRecord)
            .ThenInclude(x => x!.BeneficiaryProfile)
            .AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var payments = await query.OrderBy(x => x.DueDate).Select(x => MapSponsorshipPayment(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<SponsorshipPaymentResponse>>(payments);
    }

    public async Task<Result<IEnumerable<SponsorshipPaymentResponse>>> GenerateSponsorshipPaymentsAsync(int recordId, GenerateSponsorshipPaymentsRequest request, CancellationToken cancellationToken = default)
    {
        var record = await SponsorshipRecordQuery(false).FirstOrDefaultAsync(x => x.Id == recordId, cancellationToken);
        if (record is null)
            return Result.Failure<IEnumerable<SponsorshipPaymentResponse>>(BeneficiaryServicesErrors.SponsorshipRecordNotFound);
        if (record.Status != SponsorshipStatus.Active)
            return Result.Failure<IEnumerable<SponsorshipPaymentResponse>>(BeneficiaryServicesErrors.SponsorshipRecordNotActive);
        if (record.Payments.Count != 0)
            return Result.Failure<IEnumerable<SponsorshipPaymentResponse>>(BeneficiaryServicesErrors.SponsorshipPaymentsAlreadyGenerated);
        if (record.Amount <= 0 || request.PaymentCount <= 0 || request.MonthsBetweenPayments <= 0)
            return Result.Failure<IEnumerable<SponsorshipPaymentResponse>>(BeneficiaryServicesErrors.InvalidRequest);

        var lastDueDate = request.FirstDueDate.Date.AddMonths((request.PaymentCount - 1) * request.MonthsBetweenPayments);
        if (record.EndsAt.HasValue && lastDueDate > record.EndsAt.Value.Date)
            return Result.Failure<IEnumerable<SponsorshipPaymentResponse>>(BeneficiaryServicesErrors.InvalidRequest);

        var payments = new List<SponsorshipPayment>();
        for (var index = 0; index < request.PaymentCount; index++)
        {
            payments.Add(new SponsorshipPayment
            {
                SponsorshipRecordId = record.Id,
                SponsorshipRecord = record,
                DueDate = request.FirstDueDate.Date.AddMonths(index * request.MonthsBetweenPayments),
                Amount = record.Amount,
                Status = SponsorshipPaymentStatus.Pending,
                Notes = request.Notes?.Trim()
            });
        }

        dbcontext.SponsorshipPayments.AddRange(payments);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success<IEnumerable<SponsorshipPaymentResponse>>(payments.Select(MapSponsorshipPayment).ToList());
    }

    public async Task<Result<SponsorshipPaymentResponse>> SaveSponsorshipPaymentAsync(int? id, SaveSponsorshipPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var record = await SponsorshipRecordQuery(false).FirstOrDefaultAsync(x => x.Id == request.SponsorshipRecordId, cancellationToken);
        if (record is null)
            return Result.Failure<SponsorshipPaymentResponse>(BeneficiaryServicesErrors.SponsorshipRecordNotFound);
        if (request.Amount <= 0)
            return Result.Failure<SponsorshipPaymentResponse>(BeneficiaryServicesErrors.InvalidRequest);

        SponsorshipPayment payment;
        if (id.HasValue)
        {
            payment = await dbcontext.SponsorshipPayments.Include(x => x.SponsorshipRecord).ThenInclude(x => x!.Sponsor).Include(x => x.SponsorshipRecord).ThenInclude(x => x!.BeneficiaryProfile).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (payment is null)
                return Result.Failure<SponsorshipPaymentResponse>(BeneficiaryServicesErrors.SponsorshipPaymentNotFound);
        }
        else
        {
            payment = new SponsorshipPayment { SponsorshipRecord = record };
            dbcontext.SponsorshipPayments.Add(payment);
        }

        payment.SponsorshipRecordId = request.SponsorshipRecordId;
        payment.DueDate = request.DueDate.Date;
        payment.Amount = request.Amount;
        payment.Status = request.Status;
        payment.PaidAt = request.Status == SponsorshipPaymentStatus.Paid ? DateTime.UtcNow.AddHours(3) : payment.PaidAt;
        payment.Notes = request.Notes?.Trim();

        record.Status = request.Status switch
        {
            SponsorshipPaymentStatus.Closed => SponsorshipStatus.Closed,
            SponsorshipPaymentStatus.Rejected => SponsorshipStatus.DueCollection,
            _ => record.Status
        };

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSponsorshipPayment(payment));
    }

    public async Task<Result<IEnumerable<EntitySupportResponse>>> GetEntitySupportsAsync(EntitySupportStatus? status, bool? isExternal, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EntitySupportRequests.AsNoTracking().Include(x => x.BeneficiaryEntity).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (isExternal.HasValue)
            query = query.Where(x => x.IsExternal == isExternal.Value);

        var supports = await query.OrderByDescending(x => x.CreatedAt).Select(x => MapEntitySupport(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<EntitySupportResponse>>(supports);
    }

    public async Task<Result<EntitySupportResponse>> SaveEntitySupportAsync(int? id, SaveEntitySupportRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequesterName) || string.IsNullOrWhiteSpace(request.SupportType) || request.Amount < 0)
            return Result.Failure<EntitySupportResponse>(BeneficiaryServicesErrors.InvalidRequest);
        if (request.BeneficiaryEntityId.HasValue && !await dbcontext.BeneficiaryEntities.AnyAsync(x => x.Id == request.BeneficiaryEntityId.Value, cancellationToken))
            return Result.Failure<EntitySupportResponse>(BeneficiaryErrors.EntityNotFound);

        EntitySupportRequest support;
        if (id.HasValue)
        {
            support = await dbcontext.EntitySupportRequests.Include(x => x.BeneficiaryEntity).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (support is null)
                return Result.Failure<EntitySupportResponse>(BeneficiaryServicesErrors.EntitySupportNotFound);
        }
        else
        {
            support = new EntitySupportRequest();
            dbcontext.EntitySupportRequests.Add(support);
        }

        support.BeneficiaryEntityId = request.BeneficiaryEntityId;
        support.RequesterName = request.RequesterName.Trim();
        support.SupportType = request.SupportType.Trim();
        support.Amount = request.Amount;
        support.IsExternal = request.IsExternal;
        if (request.IsExternal && support.Status == EntitySupportStatus.Pending)
            support.Status = EntitySupportStatus.External;

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(support).Reference(x => x.BeneficiaryEntity).LoadAsync(cancellationToken);
        return Result.Success(MapEntitySupport(support));
    }

    public async Task<Result<EntitySupportResponse>> DecideEntitySupportAsync(int id, DecideEntitySupportRequest request, CancellationToken cancellationToken = default)
    {
        var support = await dbcontext.EntitySupportRequests.Include(x => x.BeneficiaryEntity).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (support is null)
            return Result.Failure<EntitySupportResponse>(BeneficiaryServicesErrors.EntitySupportNotFound);

        support.Status = request.Status;
        support.DecisionNotes = request.DecisionNotes?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEntitySupport(support));
    }

    public async Task<Result<IEnumerable<CouponRequestResponse>>> GetCouponsAsync(CouponStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CouponRequests.AsNoTracking().Include(x => x.BeneficiaryProfile).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var coupons = await query.OrderByDescending(x => x.RequiredAt).Select(x => MapCoupon(x)).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<CouponRequestResponse>>(coupons);
    }

    public async Task<Result<CouponRequestResponse>> SaveCouponAsync(int? id, SaveCouponRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CouponType) || request.Amount <= 0)
            return Result.Failure<CouponRequestResponse>(BeneficiaryServicesErrors.InvalidRequest);
        if (request.BeneficiaryProfileId.HasValue && !await dbcontext.BeneficiaryProfiles.AnyAsync(x => x.Id == request.BeneficiaryProfileId.Value, cancellationToken))
            return Result.Failure<CouponRequestResponse>(BeneficiaryErrors.ProfileNotFound);

        CouponRequest coupon;
        if (id.HasValue)
        {
            coupon = await dbcontext.CouponRequests.Include(x => x.BeneficiaryProfile).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) ?? null!;
            if (coupon is null)
                return Result.Failure<CouponRequestResponse>(BeneficiaryServicesErrors.CouponNotFound);
        }
        else
        {
            coupon = new CouponRequest();
            dbcontext.CouponRequests.Add(coupon);
        }

        coupon.BeneficiaryProfileId = request.BeneficiaryProfileId;
        coupon.CouponType = request.CouponType.Trim();
        coupon.Amount = request.Amount;
        coupon.RequiredAt = request.RequiredAt ?? DateTime.UtcNow.AddHours(3);
        coupon.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(coupon).Reference(x => x.BeneficiaryProfile).LoadAsync(cancellationToken);
        return Result.Success(MapCoupon(coupon));
    }

    public async Task<Result<CouponRequestResponse>> UpdateCouponStatusAsync(int id, UpdateCouponStatusRequest request, CancellationToken cancellationToken = default)
    {
        var coupon = await dbcontext.CouponRequests.Include(x => x.BeneficiaryProfile).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
            return Result.Failure<CouponRequestResponse>(BeneficiaryServicesErrors.CouponNotFound);
        if (!IsValidCouponTransition(coupon.Status, request.Status))
            return Result.Failure<CouponRequestResponse>(BeneficiaryServicesErrors.InvalidCouponStatusTransition);

        coupon.Status = request.Status;
        coupon.Notes = request.Notes?.Trim() ?? coupon.Notes;
        var now = DateTime.UtcNow.AddHours(3);
        if (request.Status == CouponStatus.Issued)
            coupon.IssuedAt = now;
        if (request.Status == CouponStatus.Delivered)
            coupon.DeliveredAt = now;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCoupon(coupon));
    }

    private static bool IsValidCouponTransition(CouponStatus current, CouponStatus next) =>
        current == next ||
        next == CouponStatus.Cancelled && current != CouponStatus.Delivered ||
        current switch
        {
            CouponStatus.Required => next == CouponStatus.Issued,
            CouponStatus.Issued => next == CouponStatus.Approved,
            CouponStatus.Approved => next == CouponStatus.Delivered,
            _ => false
        };

    private IQueryable<SponsorshipRecord> SponsorshipRecordQuery(bool asNoTracking = true)
    {
        var query = dbcontext.SponsorshipRecords
            .Include(x => x.Sponsor)
            .Include(x => x.BeneficiaryProfile)
            .Include(x => x.SponsorshipRequirement)
            .Include(x => x.Payments)
            .AsQueryable();

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private async Task LoadAidRequestReferencesAsync(BeneficiaryAidRequest aidRequest, CancellationToken cancellationToken)
    {
        await dbcontext.Entry(aidRequest).Reference(x => x.BeneficiaryProfile).LoadAsync(cancellationToken);
        await dbcontext.Entry(aidRequest).Reference(x => x.BeneficiaryEntity).LoadAsync(cancellationToken);
    }

    private async Task LoadPaymentOrderReferencesAsync(BeneficiaryPaymentOrder order, CancellationToken cancellationToken)
    {
        await dbcontext.Entry(order).Reference(x => x.BeneficiaryProfile).LoadAsync(cancellationToken);
        await dbcontext.Entry(order).Reference(x => x.BeneficiaryAidRequest).LoadAsync(cancellationToken);
        await dbcontext.Entry(order).Reference(x => x.EntitySupportRequest).LoadAsync(cancellationToken);
    }

    private async Task LoadSponsorshipRecordReferencesAsync(SponsorshipRecord record, CancellationToken cancellationToken)
    {
        await dbcontext.Entry(record).Reference(x => x.Sponsor).LoadAsync(cancellationToken);
        await dbcontext.Entry(record).Reference(x => x.BeneficiaryProfile).LoadAsync(cancellationToken);
        await dbcontext.Entry(record).Reference(x => x.SponsorshipRequirement).LoadAsync(cancellationToken);
        await dbcontext.Entry(record).Collection(x => x.Payments).LoadAsync(cancellationToken);
    }

    private async Task<string> GenerateAidRequestNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryAidRequests.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"AID-{year}-{count:0000}";
    }

    private async Task<string> GeneratePaymentOrderNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.BeneficiaryPaymentOrders.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"ORD-{year}-{count:0000}";
    }

    private static AidRequestResponse MapAidRequest(BeneficiaryAidRequest request) =>
        new(request.Id, request.BeneficiaryProfileId, request.BeneficiaryProfile?.FullName, request.BeneficiaryEntityId, request.BeneficiaryEntity?.NameAr, request.RequestNumber, request.AidType, request.Amount, request.Description, request.Status.ToString(), request.IsExternal, request.SocialResearchNotes, request.DecisionNotes, request.CreatedAt);

    private static PaymentOrderResponse MapPaymentOrder(BeneficiaryPaymentOrder order) =>
        new(order.Id, order.BeneficiaryAidRequestId, order.EntitySupportRequestId, order.EntitySupportRequest?.RequesterName, order.BeneficiaryProfileId, order.BeneficiaryProfile?.FullName, order.OrderNumber, order.OrderType.ToString(), order.Amount, order.ItemDescription, order.Status.ToString(), order.DueDate, order.DecisionNotes, order.ClosedAt);

    private static SponsorResponse MapSponsor(Sponsor sponsor) =>
        new(sponsor.Id, sponsor.FullName, sponsor.Mobile, sponsor.Email, sponsor.Status.ToString(), sponsor.Notes);

    private static SponsorshipRequirementResponse MapRequirement(SponsorshipRequirement requirement) =>
        new(requirement.Id, requirement.Title, requirement.Amount, requirement.Frequency, requirement.Status.ToString(), requirement.Notes);

    private static SponsorshipRecordResponse MapSponsorshipRecord(SponsorshipRecord record)
    {
        var paid = record.Payments.Where(x => x.Status == SponsorshipPaymentStatus.Paid || x.Status == SponsorshipPaymentStatus.Closed).Sum(x => x.Amount);
        var pending = record.Payments.Where(x => x.Status == SponsorshipPaymentStatus.Pending).Sum(x => x.Amount);
        return new(record.Id, record.SponsorId, record.Sponsor?.FullName ?? string.Empty, record.BeneficiaryProfileId, record.BeneficiaryProfile?.FullName, record.SponsorshipRequirementId, record.SponsorshipRequirement?.Title, record.StartsAt, record.EndsAt, record.Amount, record.Status.ToString(), paid, pending, record.Notes);
    }

    private static SponsorshipPaymentResponse MapSponsorshipPayment(SponsorshipPayment payment) =>
        new(payment.Id, payment.SponsorshipRecordId, payment.SponsorshipRecord?.Sponsor?.FullName ?? string.Empty, payment.SponsorshipRecord?.BeneficiaryProfile?.FullName, payment.DueDate, payment.Amount, payment.PaidAt, payment.Status.ToString(), payment.Notes);

    private static EntitySupportResponse MapEntitySupport(EntitySupportRequest request) =>
        new(request.Id, request.BeneficiaryEntityId, request.BeneficiaryEntity?.NameAr, request.RequesterName, request.SupportType, request.Amount, request.IsExternal, request.Status.ToString(), request.DecisionNotes, request.CreatedAt);

    private static CouponRequestResponse MapCoupon(CouponRequest coupon) =>
        new(coupon.Id, coupon.BeneficiaryProfileId, coupon.BeneficiaryProfile?.FullName, coupon.CouponType, coupon.Amount, coupon.Status.ToString(), coupon.RequiredAt, coupon.IssuedAt, coupon.DeliveredAt, coupon.Notes);
}
