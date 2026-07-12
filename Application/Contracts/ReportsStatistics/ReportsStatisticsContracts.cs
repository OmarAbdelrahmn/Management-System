using Domain.Entities;

namespace Application.Contracts.ReportsStatistics;

public record ReportsStatisticsDashboardResponse(int DefinitionsCount, int ReportDefinitionsCount, int StatisticDefinitionsCount, int RunsCount, DateTime? LastGeneratedAt);
public record SystemReportDefinitionResponse(int Id, string Key, string NameAr, string Kind, string SourceDomain, bool IsActive, DateTime? LastGeneratedAt);
public record SystemReportRunResponse(int Id, int? SystemReportDefinitionId, string ReportKey, string ReportName, string Format, string? FiltersJson, int RowCount, string Status, string? RequestedBy, DateTime GeneratedAt);
public record GenerateSystemReportRequest(string ReportKey, string Format, string? FiltersJson, string? RequestedBy);
public record SystemReportExportResponse(SystemReportRunResponse Run, string FileName, string ContentType, string Content);
public record SaveSystemReportDefinitionRequest(string Key, string NameAr, SystemReportKind Kind, string SourceDomain, bool IsActive);
public record ArchiveSystemReportRunsRequest(string? ReportKey, DateTime Before, string? RequestedBy);
public record ArchiveSystemReportRunsResponse(int ArchivedCount, IEnumerable<SystemReportRunResponse> ArchivedRuns);
