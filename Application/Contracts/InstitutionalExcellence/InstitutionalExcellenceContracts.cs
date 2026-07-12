using Domain.Entities;

namespace Application.Contracts.InstitutionalExcellence;

public record InstitutionalExcellenceDashboardResponse(
    int PerformanceMeasuresCount,
    decimal AveragePerformanceAchievement,
    int ActiveGovernanceCyclesCount,
    decimal GovernanceScore,
    int OpenGovernanceTasksCount,
    int ActiveStrategicPlansCount,
    int ActiveStrategicIndicatorsCount,
    decimal StrategicAchievement);

public record PerformanceMeasureResponse(int Id, string Code, string Title, string MeasureType, decimal TargetValue, decimal ActualValue, string Unit, string ReportingPeriod, string Status, decimal AchievementPercent, string? Notes);
public record SavePerformanceMeasureRequest(string Code, string Title, PerformanceMeasureType MeasureType, decimal TargetValue, decimal ActualValue, string Unit, string ReportingPeriod, ExcellenceRecordStatus Status, string? Notes);

public record GovernanceCycleResponse(int Id, string Title, int Year, bool IsActive, DateTime? ActivatedAt, string Status, string? RoadmapNotes, decimal Score, int CriteriaCount, int TasksCount);
public record SaveGovernanceCycleRequest(string Title, int Year, bool IsActive, ExcellenceRecordStatus Status, string? RoadmapNotes);

public record GovernanceCriterionResponse(int Id, int GovernanceCycleId, string CycleTitle, string Code, string Title, decimal Weight, decimal TargetScore, decimal ActualScore, string Status, string? Answer, string? VerificationNotes, decimal? FinancialIndicatorValue, int AttachmentsCount);
public record SaveGovernanceCriterionRequest(int GovernanceCycleId, string Code, string Title, decimal Weight, decimal TargetScore, decimal ActualScore, GovernanceCriterionStatus Status, string? Answer, string? VerificationNotes, decimal? FinancialIndicatorValue);

public record GovernanceAttachmentResponse(int Id, int GovernanceCriterionId, string CriterionTitle, string FileName, string FileUrl, string? Notes, DateTime UploadedAt);
public record SaveGovernanceAttachmentRequest(int GovernanceCriterionId, string FileName, string FileUrl, string? Notes, DateTime? UploadedAt);

public record GovernanceTaskResponse(int Id, int? GovernanceCycleId, string CycleTitle, string Title, string? OwnerName, DateTime? DueDate, string Status, int ProgressPercent, string? Notes);
public record SaveGovernanceTaskRequest(int? GovernanceCycleId, string Title, string? OwnerName, DateTime? DueDate, GovernanceTaskStatus Status, int ProgressPercent, string? Notes);

public record GovernanceReportResponse(decimal Score, int CriteriaCount, int MetCriteriaCount, int AttachmentsCount, int OpenTasksCount, int CompletedTasksCount);

public record StrategicPlanResponse(int Id, string Title, DateTime StartDate, DateTime EndDate, string Status, string? Vision, string? Mission, string? Notes, int PerspectivesCount, int IndicatorsCount, decimal AchievementPercent);
public record SaveStrategicPlanRequest(string Title, DateTime StartDate, DateTime EndDate, ExcellenceRecordStatus Status, string? Vision, string? Mission, string? Notes);

public record StrategicPerspectiveResponse(int Id, int StrategicPlanId, string PlanTitle, string Name, int SortOrder, int GoalsCount);
public record SaveStrategicPerspectiveRequest(int StrategicPlanId, string Name, int SortOrder);

public record StrategicGoalResponse(int Id, int StrategicPerspectiveId, string PerspectiveName, string Title, string? Description, string? Vision2030Alignment, string? SustainabilityAlignment, int SortOrder);
public record SaveStrategicGoalRequest(int StrategicPerspectiveId, string Title, string? Description, string? Vision2030Alignment, string? SustainabilityAlignment, int SortOrder);

public record StrategicIndicatorResponse(int Id, int StrategicPlanId, string PlanTitle, int? StrategicGoalId, string GoalTitle, int? ParentIndicatorId, string Kind, string Name, decimal TargetValue, decimal ActualValue, string Unit, string? OwnerName, string? RelatedProjectName, string? RelatedProgramName, string Status, decimal AchievementPercent, string? Notes);
public record SaveStrategicIndicatorRequest(int StrategicPlanId, int? StrategicGoalId, int? ParentIndicatorId, StrategicIndicatorKind Kind, string Name, decimal TargetValue, decimal ActualValue, string Unit, string? OwnerName, string? RelatedProjectName, string? RelatedProgramName, StrategicIndicatorStatus Status, string? Notes);

public record StrategicVariableResponse(int Id, int StrategicPlanId, string PlanTitle, string Name, decimal Value, string? Source, bool IsAutomated, DateTime? LastFetchedAt);
public record SaveStrategicVariableRequest(int StrategicPlanId, string Name, decimal Value, string? Source, bool IsAutomated);
public record ApplyStrategicVariablesRequest(bool UpdateStatuses, string? Notes);
public record ApplyStrategicVariablesResponse(int StrategicPlanId, int VariablesCount, int UpdatedIndicatorsCount, decimal AchievementPercent, IEnumerable<StrategicIndicatorResponse> UpdatedIndicators);
