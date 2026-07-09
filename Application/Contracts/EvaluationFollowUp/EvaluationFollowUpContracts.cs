using Domain.Entities;

namespace Application.Contracts.EvaluationFollowUp;

public record EvaluationFollowUpDashboardResponse(int RequestedCount, int RunningCount, int PendingApprovalCount, int RejectedCount, int CompletedCount, int ActivitiesCount, int NextActionsCount);

public record FollowUpCaseResponse(
    int Id,
    string CaseNumber,
    string SubjectType,
    string SubjectName,
    string? ReferenceNumber,
    string RequestedBy,
    DateTime RequestDate,
    DateTime? DueDate,
    string Priority,
    string Status,
    string? RejectionNote,
    string? CompletionSummary,
    DateTime? CompletedAt,
    string? ApprovalNote,
    int ActivitiesCount);

public record SaveFollowUpCaseRequest(
    FollowUpSubjectType SubjectType,
    string SubjectName,
    string? ReferenceNumber,
    string RequestedBy,
    DateTime RequestDate,
    DateTime? DueDate,
    FollowUpPriority Priority,
    FollowUpCaseStatus Status,
    string? RejectionNote,
    string? CompletionSummary,
    string? ApprovalNote);

public record UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus Status, string? Note);

public record FollowUpActivityResponse(
    int Id,
    int? FollowUpCaseId,
    string CaseNumber,
    string SubjectType,
    string SubjectName,
    string? ReferenceNumber,
    DateTime ActivityDate,
    string ActivityType,
    string Summary,
    string? Result,
    string? OwnerName,
    bool RequiresNextAction,
    DateTime? NextActionDate);

public record SaveFollowUpActivityRequest(
    int? FollowUpCaseId,
    FollowUpSubjectType SubjectType,
    string SubjectName,
    string? ReferenceNumber,
    DateTime ActivityDate,
    string ActivityType,
    string Summary,
    string? Result,
    string? OwnerName,
    bool RequiresNextAction,
    DateTime? NextActionDate);
