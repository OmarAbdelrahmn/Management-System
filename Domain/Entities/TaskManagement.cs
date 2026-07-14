using Domain.Auditing;
using Domain.Identity;

namespace Domain.Entities;

public enum ManagementTaskStatus
{
    New = 0,
    InProgress = 1,
    Blocked = 2,
    Completed = 3,
    Deleted = 4
}

public enum ManagementTaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum ManagementTaskActivityType
{
    Created = 0,
    Updated = 1,
    Started = 2,
    Completed = 3,
    Redirected = 4,
    Deleted = 5,
    Restored = 6,
    Comment = 7
}

public enum ApprovalRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum ApprovalActionDecision
{
    Approved = 0,
    Rejected = 1,
    Returned = 2,
    Delegated = 3
}

public class ManagementTask : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatorUserId { get; set; } = string.Empty;
    public string AssigneeUserId { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public ManagementTaskPriority Priority { get; set; } = ManagementTaskPriority.Normal;
    public ManagementTaskStatus Status { get; set; } = ManagementTaskStatus.New;
    public int ProgressPercentage { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? CompletionNote { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? RedirectReason { get; set; }
    public string? DeletedReason { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationUser? CreatorUser { get; set; }
    public ApplicationUser? AssigneeUser { get; set; }
    public ICollection<ManagementTaskActivity> Activities { get; set; } = new List<ManagementTaskActivity>();
}

public class ManagementTaskActivity : IAuditable
{
    public int Id { get; set; }
    public int ManagementTaskId { get; set; }
    public ManagementTaskActivityType Type { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string? Note { get; set; }
    public ManagementTaskStatus? FromStatus { get; set; }
    public ManagementTaskStatus? ToStatus { get; set; }
    public string? FromAssigneeUserId { get; set; }
    public string? ToAssigneeUserId { get; set; }
    public int? ProgressPercentage { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ManagementTask? ManagementTask { get; set; }
    public ApplicationUser? ActorUser { get; set; }
    public ApplicationUser? FromAssigneeUser { get; set; }
    public ApplicationUser? ToAssigneeUser { get; set; }
}

public class ApprovalRoute : IAuditable
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DefaultDeadlineHours { get; set; } = 72;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
}

public class ApprovalStep : IAuditable
{
    public int Id { get; set; }
    public int ApprovalRouteId { get; set; }
    public int StepOrder { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string ApproverUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApprovalRoute? ApprovalRoute { get; set; }
    public ApplicationUser? ApproverUser { get; set; }
}

public class ApprovalRequest : IAuditable
{
    public int Id { get; set; }
    public int ApprovalRouteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string CurrentApproverUserId { get; set; } = string.Empty;
    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Pending;
    public int CurrentStepOrder { get; set; } = 1;
    public string? FinalComment { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? LastEscalatedAt { get; set; }
    public int EscalationCount { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApprovalRoute? ApprovalRoute { get; set; }
    public ApplicationUser? RequestedByUser { get; set; }
    public ApplicationUser? CurrentApproverUser { get; set; }
    public ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();
}

public class ApprovalAction : IAuditable
{
    public int Id { get; set; }
    public int ApprovalRequestId { get; set; }
    public int StepOrder { get; set; }
    public string ActionByUserId { get; set; } = string.Empty;
    public string? DelegatedToUserId { get; set; }
    public ApprovalActionDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApprovalRequest? ApprovalRequest { get; set; }
    public ApplicationUser? ActionByUser { get; set; }
    public ApplicationUser? DelegatedToUser { get; set; }
}
