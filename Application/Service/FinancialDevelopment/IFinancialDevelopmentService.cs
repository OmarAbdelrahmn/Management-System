using Application.Abstraction;
using Application.Contracts.FinancialDevelopment;
using Domain.Entities;

namespace Application.Service.FinancialDevelopment;

public interface IFinancialDevelopmentService
{
    Task<Result<FinancialDevelopmentDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FinancialSupporterResponse>>> GetSupportersAsync(FinancialSupporterStatus? status = null, string? search = null, CancellationToken cancellationToken = default);
    Task<Result<FinancialSupporterResponse>> SaveSupporterAsync(int? id, SaveFinancialSupporterRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FundraisingOpportunityResponse>>> GetOpportunitiesAsync(FundraisingOpportunityType? type = null, FundraisingOpportunityStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<FundraisingOpportunityResponse>> SaveOpportunityAsync(int? id, SaveFundraisingOpportunityRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DonationContributionResponse>>> GetContributionsAsync(DonationContributionStatus? status = null, int? supporterId = null, int? opportunityId = null, CancellationToken cancellationToken = default);
    Task<Result<DonationContributionResponse>> SaveContributionAsync(int? id, SaveDonationContributionRequest request, CancellationToken cancellationToken = default);
    Task<Result<DonationReportSummaryResponse>> GetDonationReportAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DigitalMarketingCampaignResponse>>> GetDigitalCampaignsAsync(DigitalMarketingCampaignStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<DigitalMarketingCampaignResponse>> SaveDigitalCampaignAsync(int? id, SaveDigitalMarketingCampaignRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AbandonedDonationCartResponse>>> GetAbandonedCartsAsync(AbandonedDonationCartStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<AbandonedDonationCartResponse>> SaveAbandonedCartAsync(int? id, SaveAbandonedDonationCartRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EndowmentAssetResponse>>> GetEndowmentsAsync(EndowmentAssetStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<EndowmentAssetResponse>> SaveEndowmentAsync(int? id, SaveEndowmentAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EndowmentContractResponse>>> GetEndowmentContractsAsync(int? endowmentAssetId = null, CancellationToken cancellationToken = default);
    Task<Result<EndowmentContractResponse>> SaveEndowmentContractAsync(int? id, SaveEndowmentContractRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EndowmentInvoiceResponse>>> GetEndowmentInvoicesAsync(EndowmentInvoiceStatus? status = null, bool dueSoonOnly = false, CancellationToken cancellationToken = default);
    Task<Result<EndowmentInvoiceResponse>> SaveEndowmentInvoiceAsync(int? id, SaveEndowmentInvoiceRequest request, CancellationToken cancellationToken = default);
}
