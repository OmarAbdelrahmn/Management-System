using Domain.Auditing;

namespace Domain.Entities;

public enum EstablishmentDocumentStatus
{
    Draft = 0,
    Active = 1,
    NeedsReview = 2,
    Archived = 3
}

public enum AidCommitteeCreditType
{
    OpeningBalance = 0,
    Allocation = 1,
    Expense = 2,
    Adjustment = 3
}

public enum ExecutiveApprovalKind
{
    PaymentAuthorization = 0,
    SponsorshipExpense = 1,
    PaymentOrder = 2,
    Coupon = 3,
    Recruitment = 4,
    Purchase = 5,
    GeneralMaintenance = 6,
    VehicleMaintenance = 7
}

public enum ExecutiveWorkflowStatus
{
    Pending = 0,
    Approved = 1,
    RejectedWithNote = 2,
    FinalRejected = 3,
    Completed = 4
}

public enum AdministrativeDecisionType
{
    Administrative = 0,
    Meeting = 1,
    DecisionTask = 2,
    ExportTemplate = 3
}

public class EstablishmentDocument : IAuditable
{
    public int Id { get; set; }
    public string DocumentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? OwnerDepartment { get; set; }
    public string? FileUrl { get; set; }
    public EstablishmentDocumentStatus Status { get; set; } = EstablishmentDocumentStatus.Draft;
    public string? HelperNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class AidCommitteeCreditEntry : IAuditable
{
    public int Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public AidCommitteeCreditType EntryType { get; set; } = AidCommitteeCreditType.Allocation;
    public decimal Amount { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class ExecutiveApprovalRequest : IAuditable
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public ExecutiveApprovalKind ApprovalKind { get; set; } = ExecutiveApprovalKind.PaymentOrder;
    public string Subject { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public ExecutiveWorkflowStatus Status { get; set; } = ExecutiveWorkflowStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class PaymentAuthorization : IAuditable
{
    public int Id { get; set; }
    public string AuthorizationNumber { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExecutiveWorkflowStatus Status { get; set; } = ExecutiveWorkflowStatus.Pending;
    public string? RejectionNote { get; set; }
    public DateTime AuthorizationDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class AdministrativeDecisionRecord : IAuditable
{
    public int Id { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public AdministrativeDecisionType DecisionType { get; set; } = AdministrativeDecisionType.Administrative;
    public string Title { get; set; } = string.Empty;
    public string? RelatedMeetingCode { get; set; }
    public string? AssignedTo { get; set; }
    public ExecutiveWorkflowStatus Status { get; set; } = ExecutiveWorkflowStatus.Pending;
    public DateTime DecisionDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? ExportTemplateName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
