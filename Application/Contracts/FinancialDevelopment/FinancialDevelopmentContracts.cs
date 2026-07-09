using Domain.Entities;

namespace Application.Contracts.FinancialDevelopment;

public record FinancialDevelopmentDashboardResponse(
    int SupportersCount,
    int ActiveOpportunitiesCount,
    decimal ConfirmedDonationsAmount,
    int ActiveCampaignsCount,
    int OpenAbandonedCartsCount,
    int ActiveEndowmentsCount,
    int DueEndowmentInvoicesCount);

public record FinancialSupporterResponse(
    int Id,
    string Name,
    string SupporterType,
    string? Category,
    string? Mobile,
    string? Email,
    string PreferredContactChannel,
    string Status,
    string? Notes);

public record SaveFinancialSupporterRequest(
    string Name,
    FinancialSupporterType SupporterType,
    string? Category,
    string? Mobile,
    string? Email,
    string? NationalIdOrRegistrationNo,
    string PreferredContactChannel,
    FinancialSupporterStatus Status,
    string? Notes);

public record FundraisingOpportunityResponse(
    int Id,
    string Title,
    string OpportunityType,
    string? ReferenceNumber,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    string? ExternalUrl,
    string? Notes);

public record SaveFundraisingOpportunityRequest(
    string Title,
    FundraisingOpportunityType OpportunityType,
    string? ReferenceNumber,
    decimal TargetAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    FundraisingOpportunityStatus Status,
    string? ExternalUrl,
    string? Notes);

public record DonationContributionResponse(
    int Id,
    int? FinancialSupporterId,
    string SupporterName,
    int? FundraisingOpportunityId,
    string OpportunityTitle,
    decimal Amount,
    DateTime DonationDate,
    string SourceChannel,
    string? PaymentMethod,
    string? TransactionReference,
    bool IsGift,
    string? GiftRecipientName,
    string? CertificateNumber,
    string Status,
    string? Notes);

public record SaveDonationContributionRequest(
    int? FinancialSupporterId,
    int? FundraisingOpportunityId,
    decimal Amount,
    DateTime DonationDate,
    string SourceChannel,
    string? PaymentMethod,
    string? TransactionReference,
    bool IsGift,
    string? GiftRecipientName,
    string? CertificateNumber,
    DonationContributionStatus Status,
    string? Notes);

public record DonationReportSummaryResponse(
    decimal TotalConfirmedAmount,
    decimal AverageConfirmedAmount,
    int ConfirmedCount,
    int GiftCount,
    int CertificateCount,
    int UniqueSupportersCount,
    IEnumerable<DonationSourceSummaryResponse> SourceSummaries);

public record DonationSourceSummaryResponse(string SourceChannel, int Count, decimal Amount);

public record DigitalMarketingCampaignResponse(
    int Id,
    string Title,
    string Channel,
    decimal Budget,
    string? TargetAudience,
    string? LandingPageUrl,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    int LeadsCount,
    int DonationsCount,
    decimal DonationsAmount,
    string? Notes);

public record SaveDigitalMarketingCampaignRequest(
    string Title,
    DigitalMarketingChannel Channel,
    decimal Budget,
    string? TargetAudience,
    string? LandingPageUrl,
    DateTime? StartDate,
    DateTime? EndDate,
    DigitalMarketingCampaignStatus Status,
    int LeadsCount,
    int DonationsCount,
    decimal DonationsAmount,
    string? Notes);

public record AbandonedDonationCartResponse(
    int Id,
    int? FundraisingOpportunityId,
    string OpportunityTitle,
    string SupporterName,
    string? Mobile,
    decimal Amount,
    DateTime CartDate,
    string Status,
    string? FollowUpNotes);

public record SaveAbandonedDonationCartRequest(
    int? FundraisingOpportunityId,
    string SupporterName,
    string? Mobile,
    decimal Amount,
    DateTime CartDate,
    AbandonedDonationCartStatus Status,
    string? FollowUpNotes);

public record EndowmentAssetResponse(
    int Id,
    string Name,
    string? EndowmentNumber,
    string AssetType,
    decimal EstimatedValue,
    decimal AnnualReturnEstimate,
    string Status,
    string? ManagerName,
    string? Notes);

public record SaveEndowmentAssetRequest(
    string Name,
    string? EndowmentNumber,
    string AssetType,
    decimal EstimatedValue,
    decimal AnnualReturnEstimate,
    EndowmentAssetStatus Status,
    string? ManagerName,
    string? Notes);

public record EndowmentContractResponse(
    int Id,
    int EndowmentAssetId,
    string EndowmentName,
    string ContractNumber,
    string LesseeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal AnnualAmount,
    string Status,
    string? Notes);

public record SaveEndowmentContractRequest(
    int EndowmentAssetId,
    string ContractNumber,
    string LesseeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal AnnualAmount,
    EndowmentContractStatus Status,
    string? Notes);

public record EndowmentInvoiceResponse(
    int Id,
    int EndowmentAssetId,
    string EndowmentName,
    int? EndowmentContractId,
    string? ContractNumber,
    string InvoiceNumber,
    DateTime DueDate,
    decimal Amount,
    decimal PaidAmount,
    string Status,
    string? Notes);

public record SaveEndowmentInvoiceRequest(
    int EndowmentAssetId,
    int? EndowmentContractId,
    string InvoiceNumber,
    DateTime DueDate,
    decimal Amount,
    decimal PaidAmount,
    EndowmentInvoiceStatus Status,
    string? Notes);
