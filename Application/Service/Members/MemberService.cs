using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Members;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Members;

public class MemberService(ApplicationDbcontext dbcontext) : IMemberService
{
    public async Task<Result<IEnumerable<MembershipTypeResponse>>> GetMembershipTypesAsync(CancellationToken cancellationToken = default)
    {
        var types = await dbcontext.MembershipTypes
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.NameAr)
            .Select(x => MapType(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MembershipTypeResponse>>(types);
    }

    public async Task<Result<MembershipTypeResponse>> CreateMembershipTypeAsync(UpsertMembershipTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameAr) || request.AnnualFee < 0 || request.VotingWeight <= 0)
            return Result.Failure<MembershipTypeResponse>(MemberErrors.InvalidRequest);

        var type = new MembershipType
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn?.Trim(),
            AnnualFee = request.AnnualFee,
            VotingWeight = request.VotingWeight,
            IsSupporterType = request.IsSupporterType,
            IsActive = request.IsActive
        };

        dbcontext.MembershipTypes.Add(type);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapType(type));
    }

    public async Task<Result<MembershipTypeResponse>> UpdateMembershipTypeAsync(int id, UpsertMembershipTypeRequest request, CancellationToken cancellationToken = default)
    {
        var type = await dbcontext.MembershipTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (type is null)
            return Result.Failure<MembershipTypeResponse>(MemberErrors.MembershipTypeNotFound);

        if (string.IsNullOrWhiteSpace(request.NameAr) || request.AnnualFee < 0 || request.VotingWeight <= 0)
            return Result.Failure<MembershipTypeResponse>(MemberErrors.InvalidRequest);

        type.NameAr = request.NameAr.Trim();
        type.NameEn = request.NameEn?.Trim();
        type.AnnualFee = request.AnnualFee;
        type.VotingWeight = request.VotingWeight;
        type.IsSupporterType = request.IsSupporterType;
        type.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapType(type));
    }

    public async Task<Result<IEnumerable<MemberResponse>>> SearchAsync(MemberSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MemberProfiles
            .AsNoTracking()
            .Include(x => x.MembershipType)
            .Include(x => x.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.MemberNumber.Contains(search) ||
                (x.NationalId != null && x.NationalId.Contains(search)) ||
                (x.Email != null && x.Email.Contains(search)) ||
                (x.Mobile != null && x.Mobile.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.MembershipTypeId.HasValue)
            query = query.Where(x => x.MembershipTypeId == request.MembershipTypeId.Value);

        if (request.FeesPaid.HasValue)
            query = query.Where(x => x.FeesPaid == request.FeesPaid.Value);

        if (request.IsSupporter.HasValue)
            query = query.Where(x => x.IsSupporter == request.IsSupporter.Value);

        var members = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.FullName)
            .Select(x => MapMember(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberResponse>>(members);
    }

    public async Task<Result<MemberResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles
            .AsNoTracking()
            .Include(x => x.MembershipType)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return member is null
            ? Result.Failure<MemberResponse>(MemberErrors.NotFound)
            : Result.Success(MapMember(member));
    }

    public async Task<Result<MemberResponse>> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken = default)
    {
        var type = await dbcontext.MembershipTypes.FirstOrDefaultAsync(x => x.Id == request.MembershipTypeId && x.IsActive, cancellationToken);
        if (type is null)
            return Result.Failure<MemberResponse>(MemberErrors.MembershipTypeNotFound);

        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result.Failure<MemberResponse>(MemberErrors.InvalidRequest);

        var memberNumber = string.IsNullOrWhiteSpace(request.MemberNumber)
            ? await GenerateMemberNumberAsync(cancellationToken)
            : request.MemberNumber.Trim();

        if (await dbcontext.MemberProfiles.AnyAsync(x => x.MemberNumber == memberNumber, cancellationToken))
            return Result.Failure<MemberResponse>(MemberErrors.DuplicateMemberNumber);

        var member = new MemberProfile
        {
            MemberNumber = memberNumber,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId?.Trim(),
            Email = request.Email?.Trim(),
            Mobile = request.Mobile?.Trim(),
            City = request.City?.Trim(),
            Address = request.Address?.Trim(),
            MembershipTypeId = request.MembershipTypeId,
            ApplicationUserId = string.IsNullOrWhiteSpace(request.ApplicationUserId) ? null : request.ApplicationUserId,
            JoinedAt = request.JoinedAt ?? DateTime.UtcNow.AddHours(3),
            FeesPaid = request.FeesPaid,
            IsSupporter = request.IsSupporter,
            CumulativePercentage = request.IsSupporter ? Math.Max(request.CumulativePercentage, 1) : 1,
            Notes = request.Notes?.Trim()
        };

        dbcontext.MemberProfiles.Add(member);
        await dbcontext.SaveChangesAsync(cancellationToken);

        member.MembershipType = type;
        return Result.Success(MapMember(member));
    }

    public async Task<Result<MemberResponse>> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles
            .Include(x => x.MembershipType)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (member is null)
            return Result.Failure<MemberResponse>(MemberErrors.NotFound);

        var typeExists = await dbcontext.MembershipTypes.AnyAsync(x => x.Id == request.MembershipTypeId && x.IsActive, cancellationToken);
        if (!typeExists)
            return Result.Failure<MemberResponse>(MemberErrors.MembershipTypeNotFound);

        member.FullName = request.FullName.Trim();
        member.NationalId = request.NationalId?.Trim();
        member.Email = request.Email?.Trim();
        member.Mobile = request.Mobile?.Trim();
        member.City = request.City?.Trim();
        member.Address = request.Address?.Trim();
        member.ApplicationUserId = string.IsNullOrWhiteSpace(request.ApplicationUserId) ? null : request.ApplicationUserId;
        member.MembershipTypeId = request.MembershipTypeId;
        member.JoinedAt = request.JoinedAt;
        member.FeesPaid = request.FeesPaid;
        member.IsSupporter = request.IsSupporter;
        member.CumulativePercentage = request.IsSupporter ? Math.Max(request.CumulativePercentage, 1) : 1;
        member.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(member).Reference(x => x.MembershipType).LoadAsync(cancellationToken);
        return Result.Success(MapMember(member));
    }

    public async Task<Result> CancelAsync(int id, CancelMemberRequest request, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (member is null)
            return Result.Failure(MemberErrors.NotFound);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(MemberErrors.InvalidRequest);

        member.Status = MemberStatus.Cancelled;
        member.CancelledAt = DateTime.UtcNow.AddHours(3);
        member.CancellationReason = request.Reason.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (member is null)
            return Result.Failure(MemberErrors.NotFound);

        member.Status = MemberStatus.Active;
        member.CancelledAt = null;
        member.CancellationReason = null;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<MemberPaymentResponse>> RecordPaymentAsync(int memberId, RecordMemberPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles.FirstOrDefaultAsync(x => x.Id == memberId, cancellationToken);
        if (member is null)
            return Result.Failure<MemberPaymentResponse>(MemberErrors.NotFound);

        if (request.Amount <= 0)
            return Result.Failure<MemberPaymentResponse>(MemberErrors.InvalidRequest);

        var payment = new MemberPayment
        {
            MemberProfileId = memberId,
            Amount = request.Amount,
            DueDate = request.DueDate,
            PaidAt = request.MarkAsPaid ? request.PaidAt ?? DateTime.UtcNow.AddHours(3) : null,
            Status = request.MarkAsPaid ? MemberPaymentStatus.Paid : MemberPaymentStatus.Pending,
            ReceiptNumber = request.ReceiptNumber?.Trim(),
            Notes = request.Notes?.Trim()
        };

        dbcontext.MemberPayments.Add(payment);
        await dbcontext.SaveChangesAsync(cancellationToken);

        member.FeesPaid = await dbcontext.MemberPayments.AnyAsync(x => x.MemberProfileId == memberId && x.Status == MemberPaymentStatus.Paid, cancellationToken);
        await dbcontext.SaveChangesAsync(cancellationToken);

        payment.MemberProfile = member;
        return Result.Success(MapPayment(payment));
    }

    public async Task<Result> CancelPaymentAsync(int id, CancelMemberPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await dbcontext.MemberPayments.Include(x => x.MemberProfile).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (payment is null)
            return Result.Failure(MemberErrors.PaymentNotFound);

        if (payment.Status != MemberPaymentStatus.Pending || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(MemberErrors.InvalidRequest);

        payment.Status = MemberPaymentStatus.Cancelled;
        payment.Notes = request.Reason.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);

        if (payment.MemberProfile is not null)
        {
            payment.MemberProfile.FeesPaid = await dbcontext.MemberPayments.AnyAsync(x => x.MemberProfileId == payment.MemberProfileId && x.Status == MemberPaymentStatus.Paid, cancellationToken);
            await dbcontext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> SettlePaymentAsync(int id, SettleMemberPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await dbcontext.MemberPayments.Include(x => x.MemberProfile).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (payment is null)
            return Result.Failure(MemberErrors.PaymentNotFound);

        if (payment.Status != MemberPaymentStatus.Pending)
            return Result.Failure(MemberErrors.InvalidRequest);

        payment.Status = MemberPaymentStatus.Paid;
        payment.PaidAt = request.PaidAt ?? DateTime.UtcNow.AddHours(3);
        payment.ReceiptNumber = string.IsNullOrWhiteSpace(request.ReceiptNumber) ? payment.ReceiptNumber : request.ReceiptNumber.Trim();
        payment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? payment.Notes : request.Notes.Trim();
        if (payment.MemberProfile is not null)
            payment.MemberProfile.FeesPaid = true;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<MemberPaymentResponse>>> GetPaymentsAsync(int? memberId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MemberPayments.AsNoTracking().Include(x => x.MemberProfile).AsQueryable();
        if (memberId.HasValue)
            query = query.Where(x => x.MemberProfileId == memberId.Value);

        var payments = await query
            .OrderByDescending(x => x.DueDate)
            .Select(x => MapPayment(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberPaymentResponse>>(payments);
    }

    public async Task<Result<IEnumerable<MemberResponse>>> GetDueMembersAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.AddHours(3).Date;
        var members = await dbcontext.MemberProfiles
            .AsNoTracking()
            .Include(x => x.MembershipType)
            .Include(x => x.Payments)
            .Where(x => x.Status == MemberStatus.Active && (!x.FeesPaid || x.Payments.Any(p => p.Status == MemberPaymentStatus.Pending && p.DueDate.Date < today)))
            .OrderBy(x => x.FullName)
            .Select(x => MapMember(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberResponse>>(members);
    }

    public async Task<Result<MemberCardResponse>> IssueCardAsync(int memberId, IssueMemberCardRequest request, CancellationToken cancellationToken = default)
    {
        var member = await dbcontext.MemberProfiles.FirstOrDefaultAsync(x => x.Id == memberId, cancellationToken);
        if (member is null)
            return Result.Failure<MemberCardResponse>(MemberErrors.NotFound);

        var now = DateTime.UtcNow.AddHours(3);
        var activeCards = await dbcontext.MemberCards.Where(x => x.MemberProfileId == memberId && x.IsActive).ToListAsync(cancellationToken);
        foreach (var activeCard in activeCards)
            activeCard.IsActive = false;

        var card = new MemberCard
        {
            MemberProfileId = memberId,
            CardNumber = $"CARD-{member.MemberNumber}-{now:yyyyMMddHHmmss}",
            IssuedAt = now,
            ExpiresAt = request.ExpiresAt ?? now.AddYears(1),
            IsActive = true
        };

        dbcontext.MemberCards.Add(card);
        await dbcontext.SaveChangesAsync(cancellationToken);

        card.MemberProfile = member;
        return Result.Success(MapCard(card));
    }

    public async Task<Result> DeactivateCardAsync(int id, CancellationToken cancellationToken = default)
    {
        var card = await dbcontext.MemberCards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (card is null)
            return Result.Failure(MemberErrors.CardNotFound);

        card.IsActive = false;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<MemberCardResponse>>> GetCardsAsync(CancellationToken cancellationToken = default)
    {
        var cards = await dbcontext.MemberCards
            .AsNoTracking()
            .Include(x => x.MemberProfile)
            .OrderByDescending(x => x.IssuedAt)
            .Select(x => MapCard(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberCardResponse>>(cards);
    }

    public async Task<Result<MemberReportShareResponse>> ShareReportAsync(ShareMemberReportRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<MemberReportShareResponse>(MemberErrors.InvalidRequest);

        var recipientCount = request.Audience.Equals("DueMembers", StringComparison.OrdinalIgnoreCase)
            ? await dbcontext.MemberProfiles.CountAsync(x => x.Status == MemberStatus.Active && !x.FeesPaid, cancellationToken)
            : await dbcontext.MemberProfiles.CountAsync(x => x.Status == MemberStatus.Active, cancellationToken);

        var share = new MemberReportShare
        {
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Audience = string.IsNullOrWhiteSpace(request.Audience) ? "AllActiveMembers" : request.Audience.Trim(),
            RecipientCount = recipientCount,
            SharedAt = DateTime.UtcNow.AddHours(3)
        };

        dbcontext.MemberReportShares.Add(share);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapReportShare(share));
    }

    public async Task<Result<IEnumerable<MemberReportShareResponse>>> GetReportSharesAsync(CancellationToken cancellationToken = default)
    {
        var shares = await dbcontext.MemberReportShares
            .AsNoTracking()
            .OrderByDescending(x => x.SharedAt)
            .Select(x => MapReportShare(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberReportShareResponse>>(shares);
    }

    public async Task<Result<IEnumerable<MemberParticipationResponse>>> GetParticipationAssignmentsAsync(
        MemberParticipationSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MemberParticipationAssignments
            .AsNoTracking()
            .Include(x => x.MemberProfile)
            .AsQueryable();

        if (request.Role.HasValue)
            query = query.Where(x => x.Role == request.Role.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.MemberProfileId.HasValue)
            query = query.Where(x => x.MemberProfileId == request.MemberProfileId.Value);

        var assignments = await query
            .OrderBy(x => x.Role)
            .ThenByDescending(x => x.Status == MemberParticipationStatus.Active)
            .ThenBy(x => x.MemberProfile!.FullName)
            .Select(x => MapParticipation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MemberParticipationResponse>>(assignments);
    }

    public async Task<Result<MemberParticipationResponse>> SaveParticipationAssignmentAsync(
        int? id,
        SaveMemberParticipationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.MemberProfileId <= 0 || request.VotingWeight <= 0 || request.EndsAt.HasValue && request.EndsAt.Value.Date < request.StartsAt.Date)
            return Result.Failure<MemberParticipationResponse>(MemberErrors.InvalidRequest);

        var member = await dbcontext.MemberProfiles.FirstOrDefaultAsync(x => x.Id == request.MemberProfileId, cancellationToken);
        if (member is null)
            return Result.Failure<MemberParticipationResponse>(MemberErrors.NotFound);

        if (request.Status == MemberParticipationStatus.Active)
        {
            var duplicateActive = await dbcontext.MemberParticipationAssignments.AnyAsync(x =>
                x.MemberProfileId == request.MemberProfileId &&
                x.Role == request.Role &&
                x.Status == MemberParticipationStatus.Active &&
                (!id.HasValue || x.Id != id.Value),
                cancellationToken);

            if (duplicateActive)
                return Result.Failure<MemberParticipationResponse>(MemberErrors.DuplicateActiveParticipation);
        }

        MemberParticipationAssignment assignment;
        if (id.HasValue)
        {
            assignment = await dbcontext.MemberParticipationAssignments
                .Include(x => x.MemberProfile)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
                ?? null!;
            if (assignment is null)
                return Result.Failure<MemberParticipationResponse>(MemberErrors.ParticipationNotFound);
        }
        else
        {
            assignment = new MemberParticipationAssignment();
            dbcontext.MemberParticipationAssignments.Add(assignment);
        }

        assignment.MemberProfileId = request.MemberProfileId;
        assignment.Role = request.Role;
        assignment.PositionTitle = request.PositionTitle?.Trim();
        assignment.CycleName = request.CycleName?.Trim();
        assignment.StartsAt = request.StartsAt.Date;
        assignment.EndsAt = request.EndsAt?.Date;
        assignment.Status = request.Status;
        assignment.VotingWeight = request.VotingWeight;
        assignment.Notes = request.Notes?.Trim();
        assignment.MemberProfile = member;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapParticipation(assignment));
    }

    public async Task<Result<MemberParticipationResponse>> EndParticipationAssignmentAsync(
        int id,
        EndMemberParticipationRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await dbcontext.MemberParticipationAssignments
            .Include(x => x.MemberProfile)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (assignment is null)
            return Result.Failure<MemberParticipationResponse>(MemberErrors.ParticipationNotFound);

        var endDate = request.EndsAt?.Date ?? DateTime.UtcNow.AddHours(3).Date;
        if (endDate < assignment.StartsAt.Date)
            return Result.Failure<MemberParticipationResponse>(MemberErrors.InvalidRequest);

        assignment.EndsAt = endDate;
        assignment.Status = MemberParticipationStatus.Ended;
        assignment.Notes = request.Notes?.Trim() ?? assignment.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapParticipation(assignment));
    }

    private async Task<string> GenerateMemberNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.MemberProfiles.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"M-{year}-{count:0000}";
    }

    private static MembershipTypeResponse MapType(MembershipType type) =>
        new(type.Id, type.NameAr, type.NameEn, type.AnnualFee, type.VotingWeight, type.IsSupporterType, type.IsActive);

    private static MemberResponse MapMember(MemberProfile member) =>
        new(
            member.Id,
            member.MemberNumber,
            member.FullName,
            member.NationalId,
            member.Email,
            member.Mobile,
            member.City,
            member.Address,
            member.MembershipTypeId,
            member.MembershipType?.NameAr ?? string.Empty,
            member.ApplicationUserId,
            member.JoinedAt,
            member.Status.ToString(),
            member.FeesPaid,
            member.IsSupporter,
            member.CumulativePercentage,
            member.Notes,
            member.CancelledAt,
            member.CancellationReason,
            member.Payments.Where(x => x.Status == MemberPaymentStatus.Paid).Sum(x => x.Amount),
            member.CreatedAt);

    private static MemberPaymentResponse MapPayment(MemberPayment payment) =>
        new(
            payment.Id,
            payment.MemberProfileId,
            payment.MemberProfile?.FullName ?? string.Empty,
            payment.Amount,
            payment.DueDate,
            payment.PaidAt,
            payment.Status.ToString(),
            payment.ReceiptNumber,
            payment.Notes);

    private static MemberCardResponse MapCard(MemberCard card) =>
        new(
            card.Id,
            card.MemberProfileId,
            card.MemberProfile?.FullName ?? string.Empty,
            card.CardNumber,
            card.IssuedAt,
            card.ExpiresAt,
            card.IsActive);

    private static MemberReportShareResponse MapReportShare(MemberReportShare share) =>
        new(share.Id, share.Title, share.Body, share.Audience, share.SharedAt, share.RecipientCount);

    private static MemberParticipationResponse MapParticipation(MemberParticipationAssignment assignment) =>
        new(
            assignment.Id,
            assignment.MemberProfileId,
            assignment.MemberProfile?.MemberNumber ?? string.Empty,
            assignment.MemberProfile?.FullName ?? string.Empty,
            assignment.Role.ToString(),
            assignment.PositionTitle,
            assignment.CycleName,
            assignment.StartsAt,
            assignment.EndsAt,
            assignment.Status.ToString(),
            assignment.VotingWeight,
            assignment.Notes);
}
