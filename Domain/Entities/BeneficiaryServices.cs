using Domain.Auditing;

namespace Domain.Entities;

public enum AidRequestStatus
{
    Draft = 0,
    InitialApproved = 1,
    NeedsSocialResearch = 2,
    ResearchApproved = 3,
    ManagerApproved = 4,
    CommitteeApproved = 5,
    Transferred = 6,
    RejectedWithNote = 7,
    Rejected = 8,
    Closed = 9,
    External = 10
}

public enum PaymentOrderType
{
    Finance = 0,
    Storage = 1
}

public enum PaymentOrderStatus
{
    Pending = 0,
    Approved = 1,
    RejectedWithNote = 2,
    Rejected = 3,
    Removed = 4,
    Closed = 5
}

public enum SponsorStatus
{
    Active = 0,
    Removed = 1
}

public enum SponsorshipStatus
{
    Active = 0,
    Removed = 1,
    Expired = 2,
    DueCollection = 3,
    Cancelled = 4,
    Reassigned = 5,
    Closed = 6
}

public enum SponsorshipPaymentStatus
{
    Pending = 0,
    Rejected = 1,
    Paid = 2,
    Closed = 3
}

public enum EntitySupportStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    External = 3,
    Closed = 4
}

public enum CouponStatus
{
    Required = 0,
    Issued = 1,
    Approved = 2,
    Delivered = 3,
    Cancelled = 4
}

public class BeneficiaryAidRequest : IAuditable
{
    public int Id { get; set; }
    public int? BeneficiaryProfileId { get; set; }
    public int? BeneficiaryEntityId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string AidType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public AidRequestStatus Status { get; set; } = AidRequestStatus.Draft;
    public bool IsExternal { get; set; }
    public string? SocialResearchNotes { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
    public BeneficiaryEntity? BeneficiaryEntity { get; set; }
}

public class BeneficiaryPaymentOrder : IAuditable
{
    public int Id { get; set; }
    public int? BeneficiaryAidRequestId { get; set; }
    public int? BeneficiaryProfileId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public PaymentOrderType OrderType { get; set; } = PaymentOrderType.Finance;
    public decimal Amount { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.Pending;
    public DateTime? DueDate { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryAidRequest? BeneficiaryAidRequest { get; set; }
    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
}

public class Sponsor : IAuditable
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public SponsorStatus Status { get; set; } = SponsorStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<SponsorshipRecord> SponsorshipRecords { get; set; } = new List<SponsorshipRecord>();
}

public class SponsorshipRequirement : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "Monthly";
    public SponsorshipStatus Status { get; set; } = SponsorshipStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class SponsorshipRecord : IAuditable
{
    public int Id { get; set; }
    public int SponsorId { get; set; }
    public int? BeneficiaryProfileId { get; set; }
    public int? SponsorshipRequirementId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public decimal Amount { get; set; }
    public SponsorshipStatus Status { get; set; } = SponsorshipStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Sponsor? Sponsor { get; set; }
    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
    public SponsorshipRequirement? SponsorshipRequirement { get; set; }
    public ICollection<SponsorshipPayment> Payments { get; set; } = new List<SponsorshipPayment>();
}

public class SponsorshipPayment : IAuditable
{
    public int Id { get; set; }
    public int SponsorshipRecordId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public SponsorshipPaymentStatus Status { get; set; } = SponsorshipPaymentStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public SponsorshipRecord? SponsorshipRecord { get; set; }
}

public class EntitySupportRequest : IAuditable
{
    public int Id { get; set; }
    public int? BeneficiaryEntityId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string SupportType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsExternal { get; set; }
    public EntitySupportStatus Status { get; set; } = EntitySupportStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryEntity? BeneficiaryEntity { get; set; }
}

public class CouponRequest : IAuditable
{
    public int Id { get; set; }
    public int? BeneficiaryProfileId { get; set; }
    public string CouponType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CouponStatus Status { get; set; } = CouponStatus.Required;
    public DateTime RequiredAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? IssuedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
}
