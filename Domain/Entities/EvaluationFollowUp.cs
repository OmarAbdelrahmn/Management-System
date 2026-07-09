using Domain.Auditing;

namespace Domain.Entities;

public enum FollowUpCaseStatus
{
    Requested = 0,
    Rejected = 1,
    Running = 2,
    Completed = 3,
    PendingApproval = 4,
    Approved = 5,
    Archived = 6
}

public enum FollowUpPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum FollowUpSubjectType
{
    General = 0,
    Beneficiary = 1,
    Relative = 2,
    AidRequest = 3,
    Sponsor = 4,
    Sponsorship = 5,
    Supporter = 6
}

public class FollowUpCase : IAuditable
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public FollowUpSubjectType SubjectType { get; set; } = FollowUpSubjectType.General;
    public string SubjectName { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? DueDate { get; set; }
    public FollowUpPriority Priority { get; set; } = FollowUpPriority.Normal;
    public FollowUpCaseStatus Status { get; set; } = FollowUpCaseStatus.Requested;
    public string? RejectionNote { get; set; }
    public string? CompletionSummary { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ApprovalNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<FollowUpActivity> Activities { get; set; } = new List<FollowUpActivity>();
}

public class FollowUpActivity : IAuditable
{
    public int Id { get; set; }
    public int? FollowUpCaseId { get; set; }
    public FollowUpSubjectType SubjectType { get; set; } = FollowUpSubjectType.General;
    public string SubjectName { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string ActivityType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? OwnerName { get; set; }
    public bool RequiresNextAction { get; set; }
    public DateTime? NextActionDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FollowUpCase? FollowUpCase { get; set; }
}
