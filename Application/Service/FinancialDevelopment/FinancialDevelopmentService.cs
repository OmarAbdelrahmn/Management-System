using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Accounting;
using Application.Contracts.FinancialDevelopment;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.FinancialDevelopment;

public class FinancialDevelopmentService(ApplicationDbcontext dbcontext) : IFinancialDevelopmentService
{
    public async Task<Result<FinancialDevelopmentDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.AddHours(3).Date;

        return Result.Success(new FinancialDevelopmentDashboardResponse(
            await dbcontext.FinancialSupporters.CountAsync(cancellationToken),
            await dbcontext.FundraisingOpportunities.CountAsync(x => x.Status == FundraisingOpportunityStatus.Active, cancellationToken),
            await dbcontext.DonationContributions.Where(x => x.Status == DonationContributionStatus.Confirmed).SumAsync(x => x.Amount, cancellationToken),
            await dbcontext.DigitalMarketingCampaigns.CountAsync(x => x.Status == DigitalMarketingCampaignStatus.Running, cancellationToken),
            await dbcontext.AbandonedDonationCarts.CountAsync(x => x.Status == AbandonedDonationCartStatus.Open, cancellationToken),
            await dbcontext.EndowmentAssets.CountAsync(x => x.Status == EndowmentAssetStatus.Active, cancellationToken),
            await dbcontext.EndowmentInvoices.CountAsync(x => x.Status == EndowmentInvoiceStatus.Due && x.DueDate <= now.AddDays(30), cancellationToken)));
    }

    public async Task<Result<IEnumerable<FinancialSupporterResponse>>> GetSupportersAsync(FinancialSupporterStatus? status = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FinancialSupporters.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Mobile != null && x.Mobile.Contains(term) || x.Email != null && x.Email.Contains(term));
        }

        return Result.Success<IEnumerable<FinancialSupporterResponse>>(await query.OrderBy(x => x.Name).Select(x => MapSupporter(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FinancialSupporterResponse>> SaveSupporterAsync(int? id, SaveFinancialSupporterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<FinancialSupporterResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.FinancialSupporters, id, cancellationToken);
        if (entity is null)
            return Result.Failure<FinancialSupporterResponse>(FinancialDevelopmentErrors.SupporterNotFound);

        entity.Name = request.Name.Trim();
        entity.SupporterType = request.SupporterType;
        entity.Category = request.Category?.Trim();
        entity.Mobile = request.Mobile?.Trim();
        entity.Email = request.Email?.Trim();
        entity.NationalIdOrRegistrationNo = request.NationalIdOrRegistrationNo?.Trim();
        entity.PreferredContactChannel = string.IsNullOrWhiteSpace(request.PreferredContactChannel) ? "SMS" : request.PreferredContactChannel.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapSupporter(entity));
    }

    public async Task<Result<IEnumerable<FundraisingOpportunityResponse>>> GetOpportunitiesAsync(FundraisingOpportunityType? type = null, FundraisingOpportunityStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FundraisingOpportunities.AsNoTracking().AsQueryable();
        if (type.HasValue) query = query.Where(x => x.OpportunityType == type.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<FundraisingOpportunityResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapOpportunity(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FundraisingOpportunityResponse>> SaveOpportunityAsync(int? id, SaveFundraisingOpportunityRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.TargetAmount < 0)
            return Result.Failure<FundraisingOpportunityResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.FundraisingOpportunities, id, cancellationToken);
        if (entity is null)
            return Result.Failure<FundraisingOpportunityResponse>(FinancialDevelopmentErrors.OpportunityNotFound);

        entity.Title = request.Title.Trim();
        entity.OpportunityType = request.OpportunityType;
        entity.ReferenceNumber = request.ReferenceNumber?.Trim();
        entity.TargetAmount = request.TargetAmount;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.ExternalUrl = request.ExternalUrl?.Trim();
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await RecalculateOpportunityAmountAsync(entity.Id, cancellationToken);

        return Result.Success(MapOpportunity(entity));
    }

    public async Task<Result<FundraisingOpportunityResponse>> CompleteOpportunityAsync(int id, CompleteFundraisingOpportunityRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.FundraisingOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<FundraisingOpportunityResponse>(FinancialDevelopmentErrors.OpportunityNotFound);
        if (entity.Status == FundraisingOpportunityStatus.Completed)
            return Result.Success(MapOpportunity(entity));
        if (entity.Status != FundraisingOpportunityStatus.Active)
            return Result.Failure<FundraisingOpportunityResponse>(FinancialDevelopmentErrors.OpportunityNotActive);

        entity.CurrentAmount = await dbcontext.DonationContributions
            .Where(x => x.FundraisingOpportunityId == id && x.Status == DonationContributionStatus.Confirmed)
            .SumAsync(x => x.Amount, cancellationToken);
        if (entity.TargetAmount <= 0 || entity.CurrentAmount < entity.TargetAmount)
            return Result.Failure<FundraisingOpportunityResponse>(FinancialDevelopmentErrors.OpportunityTargetNotReached);

        entity.Status = FundraisingOpportunityStatus.Completed;
        entity.Notes = AppendNote(entity.Notes, request.Notes, 1500);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapOpportunity(entity));
    }

    public async Task<Result<IEnumerable<DonationContributionResponse>>> GetContributionsAsync(DonationContributionStatus? status = null, int? supporterId = null, int? opportunityId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.DonationContributions
            .AsNoTracking()
            .Include(x => x.FinancialSupporter)
            .Include(x => x.FundraisingOpportunity)
            .AsQueryable();

        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (supporterId.HasValue) query = query.Where(x => x.FinancialSupporterId == supporterId.Value);
        if (opportunityId.HasValue) query = query.Where(x => x.FundraisingOpportunityId == opportunityId.Value);

        return Result.Success<IEnumerable<DonationContributionResponse>>(await query.OrderByDescending(x => x.DonationDate).Select(x => MapContribution(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<DonationContributionResponse>> SaveContributionAsync(int? id, SaveDonationContributionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.SourceChannel))
            return Result.Failure<DonationContributionResponse>(FinancialDevelopmentErrors.InvalidRequest);

        if (request.FinancialSupporterId.HasValue && !await dbcontext.FinancialSupporters.AnyAsync(x => x.Id == request.FinancialSupporterId.Value, cancellationToken))
            return Result.Failure<DonationContributionResponse>(FinancialDevelopmentErrors.SupporterNotFound);

        if (request.FundraisingOpportunityId.HasValue && !await dbcontext.FundraisingOpportunities.AnyAsync(x => x.Id == request.FundraisingOpportunityId.Value, cancellationToken))
            return Result.Failure<DonationContributionResponse>(FinancialDevelopmentErrors.OpportunityNotFound);

        var isNew = !id.HasValue;
        var entity = await FindOrCreateAsync(dbcontext.DonationContributions, id, cancellationToken);
        if (entity is null)
            return Result.Failure<DonationContributionResponse>(FinancialDevelopmentErrors.ContributionNotFound);

        var previousOpportunityId = entity.FundraisingOpportunityId;
        var previousStatus = isNew ? (DonationContributionStatus?)null : entity.Status;
        entity.FinancialSupporterId = request.FinancialSupporterId;
        entity.FundraisingOpportunityId = request.FundraisingOpportunityId;
        entity.Amount = request.Amount;
        entity.DonationDate = request.DonationDate;
        entity.SourceChannel = request.SourceChannel.Trim();
        entity.PaymentMethod = request.PaymentMethod?.Trim();
        entity.TransactionReference = request.TransactionReference?.Trim();
        entity.IsGift = request.IsGift;
        entity.GiftRecipientName = request.GiftRecipientName?.Trim();
        entity.CertificateNumber = request.CertificateNumber?.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        QueueContributionActivity(entity.Id, isNew ? DonationContributionActivityType.Created : DonationContributionActivityType.Updated, previousStatus, entity.Status, entity.Amount, entity.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await RecalculateOpportunityAmountAsync(previousOpportunityId, cancellationToken);
        if (entity.FundraisingOpportunityId != previousOpportunityId)
            await RecalculateOpportunityAmountAsync(entity.FundraisingOpportunityId, cancellationToken);

        var saved = await dbcontext.DonationContributions
            .AsNoTracking()
            .Include(x => x.FinancialSupporter)
            .Include(x => x.FundraisingOpportunity)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapContribution(saved));
    }

    public async Task<Result<DonationContributionResponse>> UpdateContributionStatusAsync(int id, UpdateDonationContributionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.DonationContributions
            .Include(x => x.FinancialSupporter)
            .Include(x => x.FundraisingOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return Result.Failure<DonationContributionResponse>(FinancialDevelopmentErrors.ContributionNotFound);

        var previousStatus = entity.Status;
        entity.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            entity.Notes = request.Notes.Trim();

        QueueContributionActivity(entity.Id, DonationContributionActivityType.StatusChanged, previousStatus, entity.Status, entity.Amount, request.Notes?.Trim());
        await dbcontext.SaveChangesAsync(cancellationToken);
        await RecalculateOpportunityAmountAsync(entity.FundraisingOpportunityId, cancellationToken);

        return Result.Success(MapContribution(entity));
    }

    public async Task<Result<IEnumerable<DonationContributionActivityResponse>>> GetContributionActivitiesAsync(int contributionId, CancellationToken cancellationToken = default)
    {
        var exists = await dbcontext.DonationContributions.AnyAsync(x => x.Id == contributionId, cancellationToken);
        if (!exists)
            return Result.Failure<IEnumerable<DonationContributionActivityResponse>>(FinancialDevelopmentErrors.ContributionNotFound);

        var activities = await dbcontext.DonationContributionActivities
            .AsNoTracking()
            .Where(x => x.DonationContributionId == contributionId)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Select(x => MapContributionActivity(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DonationContributionActivityResponse>>(activities);
    }

    public async Task<Result<DonationReportSummaryResponse>> GetDonationReportAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.DonationContributions.AsNoTracking().Where(x => x.Status == DonationContributionStatus.Confirmed);
        if (from.HasValue) query = query.Where(x => x.DonationDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.DonationDate <= to.Value);

        var contributions = await query.ToListAsync(cancellationToken);
        var total = contributions.Sum(x => x.Amount);
        var count = contributions.Count;
        var summaries = contributions
            .GroupBy(x => x.SourceChannel)
            .OrderByDescending(x => x.Sum(y => y.Amount))
            .Select(x => new DonationSourceSummaryResponse(x.Key, x.Count(), x.Sum(y => y.Amount)))
            .ToList();

        return Result.Success(new DonationReportSummaryResponse(
            total,
            count == 0 ? 0 : total / count,
            count,
            contributions.Count(x => x.IsGift),
            contributions.Count(x => !string.IsNullOrWhiteSpace(x.CertificateNumber)),
            contributions.Where(x => x.FinancialSupporterId.HasValue).Select(x => x.FinancialSupporterId!.Value).Distinct().Count(),
            summaries));
    }

    public async Task<Result<IEnumerable<DigitalMarketingCampaignResponse>>> GetDigitalCampaignsAsync(DigitalMarketingCampaignStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.DigitalMarketingCampaigns.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<DigitalMarketingCampaignResponse>>(await query.OrderByDescending(x => x.CreatedAt).Select(x => MapDigitalCampaign(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<DigitalMarketingCampaignResponse>> SaveDigitalCampaignAsync(int? id, SaveDigitalMarketingCampaignRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Budget < 0 || request.LeadsCount < 0 || request.DonationsCount < 0 || request.DonationsAmount < 0)
            return Result.Failure<DigitalMarketingCampaignResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.DigitalMarketingCampaigns, id, cancellationToken);
        if (entity is null)
            return Result.Failure<DigitalMarketingCampaignResponse>(FinancialDevelopmentErrors.CampaignNotFound);

        entity.Title = request.Title.Trim();
        entity.Channel = request.Channel;
        entity.Budget = request.Budget;
        entity.TargetAudience = request.TargetAudience?.Trim();
        entity.LandingPageUrl = request.LandingPageUrl?.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.LeadsCount = request.LeadsCount;
        entity.DonationsCount = request.DonationsCount;
        entity.DonationsAmount = request.DonationsAmount;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDigitalCampaign(entity));
    }

    public async Task<Result<DigitalCampaignDonationResponse>> RecordDigitalCampaignDonationAsync(int id, RecordDigitalCampaignDonationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || request.Status is not (DonationContributionStatus.Pending or DonationContributionStatus.Confirmed))
            return Result.Failure<DigitalCampaignDonationResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var campaign = await dbcontext.DigitalMarketingCampaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null)
            return Result.Failure<DigitalCampaignDonationResponse>(FinancialDevelopmentErrors.CampaignNotFound);
        if (campaign.Status != DigitalMarketingCampaignStatus.Running)
            return Result.Failure<DigitalCampaignDonationResponse>(FinancialDevelopmentErrors.CampaignNotRunning);

        var contribution = await SaveContributionAsync(null, new SaveDonationContributionRequest(
            request.FinancialSupporterId,
            request.FundraisingOpportunityId,
            request.Amount,
            request.DonationDate ?? DateTime.UtcNow.AddHours(3),
            $"DigitalMarketing:{campaign.Channel}",
            request.PaymentMethod,
            request.TransactionReference,
            false,
            null,
            null,
            request.Status,
            BuildCampaignDonationNotes(campaign.Title, request.Notes)), cancellationToken);

        if (!contribution.IsSuccess)
            return Result.Failure<DigitalCampaignDonationResponse>(contribution.Error);

        if (request.CountAsLead)
            campaign.LeadsCount += 1;
        if (request.Status == DonationContributionStatus.Confirmed)
        {
            campaign.DonationsCount += 1;
            campaign.DonationsAmount += request.Amount;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(new DigitalCampaignDonationResponse(MapDigitalCampaign(campaign), contribution.Value));
    }

    public async Task<Result<IEnumerable<AbandonedDonationCartResponse>>> GetAbandonedCartsAsync(AbandonedDonationCartStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.AbandonedDonationCarts
            .AsNoTracking()
            .Include(x => x.FundraisingOpportunity)
            .AsQueryable();

        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<AbandonedDonationCartResponse>>(await query.OrderByDescending(x => x.CartDate).Select(x => MapAbandonedCart(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<AbandonedDonationCartResponse>> SaveAbandonedCartAsync(int? id, SaveAbandonedDonationCartRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SupporterName) || request.Amount <= 0)
            return Result.Failure<AbandonedDonationCartResponse>(FinancialDevelopmentErrors.InvalidRequest);

        if (request.FundraisingOpportunityId.HasValue && !await dbcontext.FundraisingOpportunities.AnyAsync(x => x.Id == request.FundraisingOpportunityId.Value, cancellationToken))
            return Result.Failure<AbandonedDonationCartResponse>(FinancialDevelopmentErrors.OpportunityNotFound);

        var entity = await FindOrCreateAsync(dbcontext.AbandonedDonationCarts, id, cancellationToken);
        if (entity is null)
            return Result.Failure<AbandonedDonationCartResponse>(FinancialDevelopmentErrors.AbandonedCartNotFound);

        entity.FundraisingOpportunityId = request.FundraisingOpportunityId;
        entity.SupporterName = request.SupporterName.Trim();
        entity.Mobile = request.Mobile?.Trim();
        entity.Amount = request.Amount;
        entity.CartDate = request.CartDate;
        entity.Status = request.Status;
        entity.FollowUpNotes = request.FollowUpNotes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.AbandonedDonationCarts.AsNoTracking().Include(x => x.FundraisingOpportunity).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapAbandonedCart(saved));
    }

    public async Task<Result<AbandonedDonationCartRecoveryResponse>> RecoverAbandonedCartAsync(int id, RecoverAbandonedDonationCartRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await dbcontext.AbandonedDonationCarts
            .Include(x => x.FundraisingOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (cart is null)
            return Result.Failure<AbandonedDonationCartRecoveryResponse>(FinancialDevelopmentErrors.AbandonedCartNotFound);
        if (cart.Status is AbandonedDonationCartStatus.Recovered or AbandonedDonationCartStatus.Expired)
            return Result.Failure<AbandonedDonationCartRecoveryResponse>(FinancialDevelopmentErrors.InvalidRequest);
        if (request.ContributionStatus is not (DonationContributionStatus.Pending or DonationContributionStatus.Confirmed))
            return Result.Failure<AbandonedDonationCartRecoveryResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var contribution = await SaveContributionAsync(null, new SaveDonationContributionRequest(
            request.FinancialSupporterId,
            cart.FundraisingOpportunityId,
            cart.Amount,
            request.DonationDate ?? DateTime.UtcNow.AddHours(3),
            "RecoveredCart",
            request.PaymentMethod,
            request.TransactionReference,
            false,
            null,
            null,
            request.ContributionStatus,
            request.Notes ?? cart.FollowUpNotes), cancellationToken);

        if (!contribution.IsSuccess)
            return Result.Failure<AbandonedDonationCartRecoveryResponse>(contribution.Error);

        cart.Status = AbandonedDonationCartStatus.Recovered;
        cart.FollowUpNotes = request.Notes?.Trim() ?? cart.FollowUpNotes;
        await dbcontext.SaveChangesAsync(cancellationToken);

        var savedCart = await dbcontext.AbandonedDonationCarts
            .AsNoTracking()
            .Include(x => x.FundraisingOpportunity)
            .FirstAsync(x => x.Id == cart.Id, cancellationToken);

        return Result.Success(new AbandonedDonationCartRecoveryResponse(MapAbandonedCart(savedCart), contribution.Value));
    }

    public async Task<Result<IEnumerable<EndowmentAssetResponse>>> GetEndowmentsAsync(EndowmentAssetStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EndowmentAssets.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<EndowmentAssetResponse>>(await query.OrderBy(x => x.Name).Select(x => MapEndowment(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<EndowmentAssetResponse>> SaveEndowmentAsync(int? id, SaveEndowmentAssetRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.AssetType) || request.EstimatedValue < 0 || request.AnnualReturnEstimate < 0)
            return Result.Failure<EndowmentAssetResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.EndowmentAssets, id, cancellationToken);
        if (entity is null)
            return Result.Failure<EndowmentAssetResponse>(FinancialDevelopmentErrors.EndowmentNotFound);

        entity.Name = request.Name.Trim();
        entity.EndowmentNumber = request.EndowmentNumber?.Trim();
        entity.AssetType = request.AssetType.Trim();
        entity.EstimatedValue = request.EstimatedValue;
        entity.AnnualReturnEstimate = request.AnnualReturnEstimate;
        entity.Status = request.Status;
        entity.ManagerName = request.ManagerName?.Trim();
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapEndowment(entity));
    }

    public async Task<Result<IEnumerable<EndowmentContractResponse>>> GetEndowmentContractsAsync(int? endowmentAssetId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EndowmentContracts.AsNoTracking().Include(x => x.EndowmentAsset).AsQueryable();
        if (endowmentAssetId.HasValue) query = query.Where(x => x.EndowmentAssetId == endowmentAssetId.Value);

        return Result.Success<IEnumerable<EndowmentContractResponse>>(await query.OrderByDescending(x => x.EndDate).Select(x => MapContract(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<EndowmentContractResponse>> SaveEndowmentContractAsync(int? id, SaveEndowmentContractRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ContractNumber) || string.IsNullOrWhiteSpace(request.LesseeName) || request.AnnualAmount < 0 || request.EndDate < request.StartDate)
            return Result.Failure<EndowmentContractResponse>(FinancialDevelopmentErrors.InvalidRequest);

        if (!await dbcontext.EndowmentAssets.AnyAsync(x => x.Id == request.EndowmentAssetId, cancellationToken))
            return Result.Failure<EndowmentContractResponse>(FinancialDevelopmentErrors.EndowmentNotFound);

        var entity = await FindOrCreateAsync(dbcontext.EndowmentContracts, id, cancellationToken);
        if (entity is null)
            return Result.Failure<EndowmentContractResponse>(FinancialDevelopmentErrors.ContractNotFound);

        entity.EndowmentAssetId = request.EndowmentAssetId;
        entity.ContractNumber = request.ContractNumber.Trim();
        entity.LesseeName = request.LesseeName.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.AnnualAmount = request.AnnualAmount;
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.EndowmentContracts.AsNoTracking().Include(x => x.EndowmentAsset).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapContract(saved));
    }

    public async Task<Result<IEnumerable<EndowmentInvoiceResponse>>> GetEndowmentInvoicesAsync(EndowmentInvoiceStatus? status = null, bool dueSoonOnly = false, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EndowmentInvoices
            .AsNoTracking()
            .Include(x => x.EndowmentAsset)
            .Include(x => x.EndowmentContract)
            .AsQueryable();

        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (dueSoonOnly)
        {
            var dueBy = DateTime.UtcNow.AddHours(3).Date.AddDays(30);
            query = query.Where(x => x.Status == EndowmentInvoiceStatus.Due && x.DueDate <= dueBy);
        }

        return Result.Success<IEnumerable<EndowmentInvoiceResponse>>(await query.OrderBy(x => x.DueDate).Select(x => MapInvoice(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<EndowmentInvoiceResponse>> SaveEndowmentInvoiceAsync(int? id, SaveEndowmentInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber) || request.Amount <= 0 || request.PaidAmount < 0 || request.PaidAmount > request.Amount || request.Status == EndowmentInvoiceStatus.Paid && request.PaidAmount < request.Amount)
            return Result.Failure<EndowmentInvoiceResponse>(FinancialDevelopmentErrors.InvalidRequest);

        if (!await dbcontext.EndowmentAssets.AnyAsync(x => x.Id == request.EndowmentAssetId, cancellationToken))
            return Result.Failure<EndowmentInvoiceResponse>(FinancialDevelopmentErrors.EndowmentNotFound);

        if (request.EndowmentContractId.HasValue)
        {
            var contract = await dbcontext.EndowmentContracts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.EndowmentContractId.Value, cancellationToken);
            if (contract is null)
                return Result.Failure<EndowmentInvoiceResponse>(FinancialDevelopmentErrors.ContractNotFound);
            if (contract.EndowmentAssetId != request.EndowmentAssetId)
                return Result.Failure<EndowmentInvoiceResponse>(FinancialDevelopmentErrors.InvalidRequest);
        }

        var entity = await FindOrCreateAsync(dbcontext.EndowmentInvoices, id, cancellationToken);
        if (entity is null)
            return Result.Failure<EndowmentInvoiceResponse>(FinancialDevelopmentErrors.InvoiceNotFound);

        entity.EndowmentAssetId = request.EndowmentAssetId;
        entity.EndowmentContractId = request.EndowmentContractId;
        entity.InvoiceNumber = request.InvoiceNumber.Trim();
        entity.DueDate = request.DueDate;
        entity.Amount = request.Amount;
        entity.PaidAmount = request.PaidAmount;
        entity.Status = request.PaidAmount == request.Amount ? EndowmentInvoiceStatus.Paid : request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.EndowmentInvoices.AsNoTracking().Include(x => x.EndowmentAsset).Include(x => x.EndowmentContract).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapInvoice(saved));
    }

    public async Task<Result<EndowmentInvoicePaymentResponse>> PayEndowmentInvoiceAsync(int id, PayEndowmentInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || request.ReceiptStatus is not (AccountingRecordStatus.Posted or AccountingRecordStatus.Approved))
            return Result.Failure<EndowmentInvoicePaymentResponse>(FinancialDevelopmentErrors.InvalidRequest);

        var invoice = await dbcontext.EndowmentInvoices
            .Include(x => x.EndowmentAsset)
            .Include(x => x.EndowmentContract)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (invoice is null)
            return Result.Failure<EndowmentInvoicePaymentResponse>(FinancialDevelopmentErrors.InvoiceNotFound);
        if (invoice.Status == EndowmentInvoiceStatus.Cancelled)
            return Result.Failure<EndowmentInvoicePaymentResponse>(FinancialDevelopmentErrors.InvoiceCancelled);
        if (invoice.Status == EndowmentInvoiceStatus.Paid || invoice.PaidAmount >= invoice.Amount)
            return Result.Failure<EndowmentInvoicePaymentResponse>(FinancialDevelopmentErrors.InvoiceAlreadyPaid);

        var remaining = invoice.Amount - invoice.PaidAmount;
        if (request.Amount > remaining)
            return Result.Failure<EndowmentInvoicePaymentResponse>(FinancialDevelopmentErrors.PaymentExceedsInvoiceBalance);

        var paymentDate = request.PaymentDate ?? DateTime.UtcNow.AddHours(3);
        var receipt = new ReceiptVoucher
        {
            ReceiptNumber = await GenerateEndowmentReceiptNumberAsync(cancellationToken),
            Kind = ReceiptVoucherKind.Investment,
            ReceiptDate = paymentDate,
            Amount = request.Amount,
            PayerName = ResolveEndowmentPayerName(invoice),
            ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? invoice.InvoiceNumber : request.ReferenceNumber.Trim(),
            Status = request.ReceiptStatus,
            Notes = BuildEndowmentReceiptNotes(invoice.InvoiceNumber, request.PaymentMethod, request.Notes)
        };
        dbcontext.ReceiptVouchers.Add(receipt);

        invoice.PaidAmount += request.Amount;
        invoice.Status = invoice.PaidAmount >= invoice.Amount
            ? EndowmentInvoiceStatus.Paid
            : invoice.DueDate.Date < DateTime.UtcNow.AddHours(3).Date ? EndowmentInvoiceStatus.Overdue : EndowmentInvoiceStatus.Due;

        await dbcontext.SaveChangesAsync(cancellationToken);

        var remainingAfterPayment = invoice.Amount - invoice.PaidAmount;
        return Result.Success(new EndowmentInvoicePaymentResponse(
            MapInvoice(invoice),
            MapReceipt(receipt),
            remainingAfterPayment,
            remainingAfterPayment == 0));
    }

    private async Task RecalculateOpportunityAmountAsync(int? opportunityId, CancellationToken cancellationToken)
    {
        if (!opportunityId.HasValue)
            return;

        var opportunity = await dbcontext.FundraisingOpportunities.FirstOrDefaultAsync(x => x.Id == opportunityId.Value, cancellationToken);
        if (opportunity is null)
            return;

        opportunity.CurrentAmount = await dbcontext.DonationContributions
            .Where(x => x.FundraisingOpportunityId == opportunityId.Value && x.Status == DonationContributionStatus.Confirmed)
            .SumAsync(x => x.Amount, cancellationToken);
        await dbcontext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateEndowmentReceiptNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var count = await dbcontext.ReceiptVouchers.CountAsync(x => x.CreatedAt.Year == year, cancellationToken) + 1;
        return $"RCV-END-{year}-{count:0000}";
    }

    private static string ResolveEndowmentPayerName(EndowmentInvoice invoice)
    {
        if (!string.IsNullOrWhiteSpace(invoice.EndowmentContract?.LesseeName))
            return invoice.EndowmentContract.LesseeName.Trim();
        if (!string.IsNullOrWhiteSpace(invoice.EndowmentAsset?.ManagerName))
            return invoice.EndowmentAsset.ManagerName.Trim();

        return string.IsNullOrWhiteSpace(invoice.EndowmentAsset?.Name) ? "Endowment payer" : invoice.EndowmentAsset.Name.Trim();
    }

    private static string? BuildEndowmentReceiptNotes(string invoiceNumber, string? paymentMethod, string? notes)
    {
        var parts = new List<string> { $"Endowment invoice: {invoiceNumber}" };
        if (!string.IsNullOrWhiteSpace(paymentMethod))
            parts.Add($"Payment method: {paymentMethod.Trim()}");
        if (!string.IsNullOrWhiteSpace(notes))
            parts.Add(notes.Trim());

        var combined = string.Join(" | ", parts);
        return combined.Length <= 1000 ? combined : combined[..1000];
    }

    private static string? BuildCampaignDonationNotes(string campaignTitle, string? notes)
    {
        var parts = new List<string> { $"Digital campaign: {campaignTitle}" };
        if (!string.IsNullOrWhiteSpace(notes))
            parts.Add(notes.Trim());

        var combined = string.Join(" | ", parts);
        return combined.Length <= 1000 ? combined : combined[..1000];
    }

    private static string? AppendNote(string? existingNotes, string? newNote, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(newNote))
            return existingNotes;

        var trimmedNote = newNote.Trim();
        var combined = string.IsNullOrWhiteSpace(existingNotes)
            ? trimmedNote
            : $"{existingNotes.Trim()}{Environment.NewLine}{trimmedNote}";
        return combined.Length <= maxLength ? combined : combined[..maxLength];
    }

    private static async Task<T?> FindOrCreateAsync<T>(DbSet<T> set, int? id, CancellationToken cancellationToken) where T : class, new()
    {
        if (!id.HasValue)
        {
            var created = new T();
            set.Add(created);
            return created;
        }

        return await set.FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id.Value, cancellationToken);
    }

    private void QueueContributionActivity(
        int contributionId,
        DonationContributionActivityType type,
        DonationContributionStatus? fromStatus,
        DonationContributionStatus toStatus,
        decimal amount,
        string? notes)
    {
        dbcontext.DonationContributionActivities.Add(new DonationContributionActivity
        {
            DonationContributionId = contributionId,
            Type = type,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Amount = amount,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OccurredAt = DateTime.UtcNow.AddHours(3)
        });
    }

    private static FinancialSupporterResponse MapSupporter(FinancialSupporter x) =>
        new(x.Id, x.Name, x.SupporterType.ToString(), x.Category, x.Mobile, x.Email, x.PreferredContactChannel, x.Status.ToString(), x.Notes);

    private static FundraisingOpportunityResponse MapOpportunity(FundraisingOpportunity x) =>
        new(x.Id, x.Title, x.OpportunityType.ToString(), x.ReferenceNumber, x.TargetAmount, x.CurrentAmount, x.StartDate, x.EndDate, x.Status.ToString(), x.ExternalUrl, x.Notes);

    private static DonationContributionResponse MapContribution(DonationContribution x) =>
        new(x.Id, x.FinancialSupporterId, x.FinancialSupporter?.Name ?? string.Empty, x.FundraisingOpportunityId, x.FundraisingOpportunity?.Title ?? string.Empty, x.Amount, x.DonationDate, x.SourceChannel, x.PaymentMethod, x.TransactionReference, x.IsGift, x.GiftRecipientName, x.CertificateNumber, x.Status.ToString(), x.Notes);

    private static DonationContributionActivityResponse MapContributionActivity(DonationContributionActivity x) =>
        new(x.Id, x.DonationContributionId, x.Type.ToString(), x.FromStatus?.ToString(), x.ToStatus.ToString(), x.Amount, x.Notes, x.OccurredAt);

    private static DigitalMarketingCampaignResponse MapDigitalCampaign(DigitalMarketingCampaign x) =>
        new(x.Id, x.Title, x.Channel.ToString(), x.Budget, x.TargetAudience, x.LandingPageUrl, x.StartDate, x.EndDate, x.Status.ToString(), x.LeadsCount, x.DonationsCount, x.DonationsAmount, x.Notes);

    private static AbandonedDonationCartResponse MapAbandonedCart(AbandonedDonationCart x) =>
        new(x.Id, x.FundraisingOpportunityId, x.FundraisingOpportunity?.Title ?? string.Empty, x.SupporterName, x.Mobile, x.Amount, x.CartDate, x.Status.ToString(), x.FollowUpNotes);

    private static EndowmentAssetResponse MapEndowment(EndowmentAsset x) =>
        new(x.Id, x.Name, x.EndowmentNumber, x.AssetType, x.EstimatedValue, x.AnnualReturnEstimate, x.Status.ToString(), x.ManagerName, x.Notes);

    private static EndowmentContractResponse MapContract(EndowmentContract x) =>
        new(x.Id, x.EndowmentAssetId, x.EndowmentAsset?.Name ?? string.Empty, x.ContractNumber, x.LesseeName, x.StartDate, x.EndDate, x.AnnualAmount, x.Status.ToString(), x.Notes);

    private static EndowmentInvoiceResponse MapInvoice(EndowmentInvoice x) =>
        new(x.Id, x.EndowmentAssetId, x.EndowmentAsset?.Name ?? string.Empty, x.EndowmentContractId, x.EndowmentContract?.ContractNumber, x.InvoiceNumber, x.DueDate, x.Amount, x.PaidAmount, x.Status.ToString(), x.Notes);

    private static ReceiptVoucherResponse MapReceipt(ReceiptVoucher x) =>
        new(x.Id, x.ReceiptNumber, x.Kind.ToString(), x.ReceiptDate, x.Amount, x.PayerName, x.ReferenceNumber, x.Status.ToString(), x.Notes);
}
