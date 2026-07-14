using Domain.Entities;

namespace Application.Contracts.TaskManagement;

public record UserPickerResponse(string Id, string FullName, string? Email);

public record ManagementTaskResponse(
    int Id,
    string Title,
    string? Description,
    string CreatorUserId,
    string CreatorName,
    string AssigneeUserId,
    string AssigneeName,
    DateTime? DueAt,
    string Priority,
    string Status,
    int ProgressPercentage,
    string? RelatedEntityType,
    int? RelatedEntityId,
    string? CompletionNote,
    DateTime? CompletedAt,
    string? RedirectReason,
    string? DeletedReason,
    DateTime? DeletedAt,
    DateTime CreatedAt);

public record CreateManagementTaskRequest(
    string Title,
    string? Description,
    string AssigneeUserId,
    DateTime? DueAt,
    ManagementTaskPriority Priority,
    string? RelatedEntityType,
    int? RelatedEntityId);

public record UpdateManagementTaskRequest(
    string Title,
    string? Description,
    string AssigneeUserId,
    DateTime? DueAt,
    ManagementTaskPriority Priority,
    ManagementTaskStatus Status,
    int ProgressPercentage);

public record TaskSearchRequest(
    string? Search,
    ManagementTaskStatus? Status,
    string? AssigneeUserId,
    bool IncludeDeleted = false);

public record CompleteTaskRequest(int ProgressPercentage, string? CompletionNote);

public record RedirectTaskRequest(string NewAssigneeUserId, string Reason);

public record DeleteTaskRequest(string Reason);

public record AddTaskCommentRequest(string Comment);

public record ManagementTaskActivityResponse(
    int Id,
    int ManagementTaskId,
    string Type,
    string ActorUserId,
    string ActorName,
    string? Note,
    string? FromStatus,
    string? ToStatus,
    string? FromAssigneeUserId,
    string? FromAssigneeName,
    string? ToAssigneeUserId,
    string? ToAssigneeName,
    int? ProgressPercentage,
    DateTime ActionAt);

public record ApprovalStepResponse(int Id, int StepOrder, string NameAr, string ApproverUserId, string ApproverName);

public record ApprovalRouteResponse(
    int Id,
    string NameAr,
    string EntityType,
    bool IsActive,
    int DefaultDeadlineHours,
    IReadOnlyList<ApprovalStepResponse> Steps);

public record CreateApprovalRouteRequest(string NameAr, string EntityType, bool IsActive, int DefaultDeadlineHours = 72);

public record UpdateApprovalRouteRequest(string NameAr, string EntityType, bool IsActive, int DefaultDeadlineHours = 72);

public record AddApprovalStepRequest(int StepOrder, string NameAr, string ApproverUserId);

public record UpdateApprovalStepRequest(int StepOrder, string NameAr, string ApproverUserId);

public record ApprovalRequestResponse(
    int Id,
    int ApprovalRouteId,
    string RouteName,
    string Title,
    string ReferenceType,
    int? ReferenceId,
    string RequestedByUserId,
    string RequestedByName,
    string Status,
    int CurrentStepOrder,
    string? CurrentApproverName,
    string? FinalComment,
    DateTime? DueAt,
    DateTime? LastEscalatedAt,
    int EscalationCount,
    DateTime? ClosedAt,
    DateTime CreatedAt);

public record CreateApprovalRequestRequest(
    int ApprovalRouteId,
    string Title,
    string ReferenceType,
    int? ReferenceId,
    DateTime? DueAt = null);

public record DecideApprovalRequestRequest(string ActionByUserId, ApprovalActionDecision Decision, string? Comment);

public record DelegateApprovalRequestRequest(string ActionByUserId, string DelegateToUserId, string Reason);

public record CancelApprovalRequestRequest(string RequestedByUserId, string? Comment);
