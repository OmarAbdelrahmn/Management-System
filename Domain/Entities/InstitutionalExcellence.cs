using Domain.Auditing;

namespace Domain.Entities;

public enum ExcellenceRecordStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Archived = 3
}

public enum PerformanceMeasureType
{
    Quantitative = 0,
    Qualitative = 1,
    Financial = 2,
    Operational = 3
}

public enum GovernanceCriterionStatus
{
    NotStarted = 0,
    InProgress = 1,
    Met = 2,
    NotMet = 3
}

public enum GovernanceTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3
}

public enum StrategicIndicatorKind
{
    Main = 0,
    Sub = 1,
    Owned = 2,
    Project = 3,
    Program = 4
}

public enum StrategicIndicatorStatus
{
    Draft = 0,
    Active = 1,
    Achieved = 2,
    AtRisk = 3,
    Closed = 4
}

public class PerformanceMeasure : IAuditable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PerformanceMeasureType MeasureType { get; set; } = PerformanceMeasureType.Quantitative;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string Unit { get; set; } = "%";
    public string ReportingPeriod { get; set; } = string.Empty;
    public ExcellenceRecordStatus Status { get; set; } = ExcellenceRecordStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class GovernanceCycle : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public ExcellenceRecordStatus Status { get; set; } = ExcellenceRecordStatus.Draft;
    public string? RoadmapNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<GovernanceCriterion> Criteria { get; set; } = new List<GovernanceCriterion>();
    public ICollection<GovernanceTask> Tasks { get; set; } = new List<GovernanceTask>();
}

public class GovernanceCriterion : IAuditable
{
    public int Id { get; set; }
    public int GovernanceCycleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal TargetScore { get; set; }
    public decimal ActualScore { get; set; }
    public GovernanceCriterionStatus Status { get; set; } = GovernanceCriterionStatus.NotStarted;
    public string? Answer { get; set; }
    public string? VerificationNotes { get; set; }
    public decimal? FinancialIndicatorValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public GovernanceCycle? GovernanceCycle { get; set; }
    public ICollection<GovernanceAttachment> Attachments { get; set; } = new List<GovernanceAttachment>();
}

public class GovernanceAttachment : IAuditable
{
    public int Id { get; set; }
    public int GovernanceCriterionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public GovernanceCriterion? GovernanceCriterion { get; set; }
}

public class GovernanceTask : IAuditable
{
    public int Id { get; set; }
    public int? GovernanceCycleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public DateTime? DueDate { get; set; }
    public GovernanceTaskStatus Status { get; set; } = GovernanceTaskStatus.Pending;
    public int ProgressPercent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public GovernanceCycle? GovernanceCycle { get; set; }
}

public class StrategicPlan : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddHours(3).Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddHours(3).Date.AddYears(1);
    public ExcellenceRecordStatus Status { get; set; } = ExcellenceRecordStatus.Draft;
    public string? Vision { get; set; }
    public string? Mission { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<StrategicPerspective> Perspectives { get; set; } = new List<StrategicPerspective>();
    public ICollection<StrategicVariable> Variables { get; set; } = new List<StrategicVariable>();
    public ICollection<StrategicIndicator> Indicators { get; set; } = new List<StrategicIndicator>();
}

public class StrategicPerspective : IAuditable
{
    public int Id { get; set; }
    public int StrategicPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public StrategicPlan? StrategicPlan { get; set; }
    public ICollection<StrategicGoal> Goals { get; set; } = new List<StrategicGoal>();
}

public class StrategicGoal : IAuditable
{
    public int Id { get; set; }
    public int StrategicPerspectiveId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Vision2030Alignment { get; set; }
    public string? SustainabilityAlignment { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public StrategicPerspective? StrategicPerspective { get; set; }
    public ICollection<StrategicIndicator> Indicators { get; set; } = new List<StrategicIndicator>();
}

public class StrategicIndicator : IAuditable
{
    public int Id { get; set; }
    public int StrategicPlanId { get; set; }
    public int? StrategicGoalId { get; set; }
    public int? ParentIndicatorId { get; set; }
    public StrategicIndicatorKind Kind { get; set; } = StrategicIndicatorKind.Main;
    public string Name { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string Unit { get; set; } = "%";
    public string? OwnerName { get; set; }
    public string? RelatedProjectName { get; set; }
    public string? RelatedProgramName { get; set; }
    public StrategicIndicatorStatus Status { get; set; } = StrategicIndicatorStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public StrategicPlan? StrategicPlan { get; set; }
    public StrategicGoal? StrategicGoal { get; set; }
    public StrategicIndicator? ParentIndicator { get; set; }
    public ICollection<StrategicIndicator> SubIndicators { get; set; } = new List<StrategicIndicator>();
}

public class StrategicVariable : IAuditable
{
    public int Id { get; set; }
    public int StrategicPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Source { get; set; }
    public bool IsAutomated { get; set; }
    public DateTime? LastFetchedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public StrategicPlan? StrategicPlan { get; set; }
}
