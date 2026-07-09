using Domain.Auditing;

namespace Domain.Entities;

public enum FinancialSupporterType
{
    Individual = 0,
    Company = 1,
    Foundation = 2,
    Government = 3,
    Other = 4
}

public enum FinancialSupporterStatus
{
    Active = 0,
    Paused = 1,
    Archived = 2
}

public enum FundraisingOpportunityType
{
    General = 0,
    Project = 1,
    AidRequest = 2,
    Program = 3,
    External = 4,
    Gift = 5,
    Certificate = 6
}

public enum FundraisingOpportunityStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Archived = 3
}

public enum DonationContributionStatus
{
    Pending = 0,
    Confirmed = 1,
    Refunded = 2,
    Cancelled = 3
}

public enum DigitalMarketingChannel
{
    Website = 0,
    Sms = 1,
    WhatsApp = 2,
    Email = 3,
    SocialMedia = 4,
    SearchAds = 5,
    Other = 6
}

public enum DigitalMarketingCampaignStatus
{
    Draft = 0,
    Running = 1,
    Completed = 2,
    Paused = 3,
    Cancelled = 4
}

public enum AbandonedDonationCartStatus
{
    Open = 0,
    Contacted = 1,
    Recovered = 2,
    Expired = 3
}

public enum EndowmentAssetStatus
{
    Draft = 0,
    Active = 1,
    Suspended = 2,
    Closed = 3
}

public enum EndowmentContractStatus
{
    Draft = 0,
    Active = 1,
    Expiring = 2,
    Closed = 3,
    Cancelled = 4
}

public enum EndowmentInvoiceStatus
{
    Due = 0,
    Paid = 1,
    Overdue = 2,
    Cancelled = 3
}

public class FinancialSupporter : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FinancialSupporterType SupporterType { get; set; } = FinancialSupporterType.Individual;
    public string? Category { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? NationalIdOrRegistrationNo { get; set; }
    public string PreferredContactChannel { get; set; } = "SMS";
    public FinancialSupporterStatus Status { get; set; } = FinancialSupporterStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<DonationContribution> Contributions { get; set; } = new List<DonationContribution>();
}

public class FundraisingOpportunity : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public FundraisingOpportunityType OpportunityType { get; set; } = FundraisingOpportunityType.General;
    public string? ReferenceNumber { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public FundraisingOpportunityStatus Status { get; set; } = FundraisingOpportunityStatus.Draft;
    public string? ExternalUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<DonationContribution> Contributions { get; set; } = new List<DonationContribution>();
    public ICollection<AbandonedDonationCart> AbandonedCarts { get; set; } = new List<AbandonedDonationCart>();
}

public class DonationContribution : IAuditable
{
    public int Id { get; set; }
    public int? FinancialSupporterId { get; set; }
    public int? FundraisingOpportunityId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DonationDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string SourceChannel { get; set; } = "Website";
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public bool IsGift { get; set; }
    public string? GiftRecipientName { get; set; }
    public string? CertificateNumber { get; set; }
    public DonationContributionStatus Status { get; set; } = DonationContributionStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FinancialSupporter? FinancialSupporter { get; set; }
    public FundraisingOpportunity? FundraisingOpportunity { get; set; }
}

public class DigitalMarketingCampaign : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DigitalMarketingChannel Channel { get; set; } = DigitalMarketingChannel.Website;
    public decimal Budget { get; set; }
    public string? TargetAudience { get; set; }
    public string? LandingPageUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DigitalMarketingCampaignStatus Status { get; set; } = DigitalMarketingCampaignStatus.Draft;
    public int LeadsCount { get; set; }
    public int DonationsCount { get; set; }
    public decimal DonationsAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class AbandonedDonationCart : IAuditable
{
    public int Id { get; set; }
    public int? FundraisingOpportunityId { get; set; }
    public string SupporterName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public decimal Amount { get; set; }
    public DateTime CartDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public AbandonedDonationCartStatus Status { get; set; } = AbandonedDonationCartStatus.Open;
    public string? FollowUpNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FundraisingOpportunity? FundraisingOpportunity { get; set; }
}

public class EndowmentAsset : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? EndowmentNumber { get; set; }
    public string AssetType { get; set; } = "RealEstate";
    public decimal EstimatedValue { get; set; }
    public decimal AnnualReturnEstimate { get; set; }
    public EndowmentAssetStatus Status { get; set; } = EndowmentAssetStatus.Draft;
    public string? ManagerName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<EndowmentContract> Contracts { get; set; } = new List<EndowmentContract>();
    public ICollection<EndowmentInvoice> Invoices { get; set; } = new List<EndowmentInvoice>();
}

public class EndowmentContract : IAuditable
{
    public int Id { get; set; }
    public int EndowmentAssetId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string LesseeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddHours(3).Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddHours(3).Date.AddYears(1);
    public decimal AnnualAmount { get; set; }
    public EndowmentContractStatus Status { get; set; } = EndowmentContractStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EndowmentAsset? EndowmentAsset { get; set; }
    public ICollection<EndowmentInvoice> Invoices { get; set; } = new List<EndowmentInvoice>();
}

public class EndowmentInvoice : IAuditable
{
    public int Id { get; set; }
    public int EndowmentAssetId { get; set; }
    public int? EndowmentContractId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddHours(3).Date;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public EndowmentInvoiceStatus Status { get; set; } = EndowmentInvoiceStatus.Due;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EndowmentAsset? EndowmentAsset { get; set; }
    public EndowmentContract? EndowmentContract { get; set; }
}
