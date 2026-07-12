using Domain.Auditing;

namespace Domain.Entities;

public enum SystemReportKind
{
    Report = 0,
    Statistic = 1
}

public enum SystemReportRunStatus
{
    Queued = 0,
    Generated = 1,
    Failed = 2,
    Archived = 3
}

public class SystemReportDefinition : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public SystemReportKind Kind { get; set; } = SystemReportKind.Report;
    public string SourceDomain { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastGeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<SystemReportRun> Runs { get; set; } = new List<SystemReportRun>();
}

public class SystemReportRun : IAuditable
{
    public int Id { get; set; }
    public int? SystemReportDefinitionId { get; set; }
    public string ReportKey { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string Format { get; set; } = "Table";
    public string? FiltersJson { get; set; }
    public int RowCount { get; set; }
    public SystemReportRunStatus Status { get; set; } = SystemReportRunStatus.Generated;
    public string? RequestedBy { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public SystemReportDefinition? SystemReportDefinition { get; set; }
}
