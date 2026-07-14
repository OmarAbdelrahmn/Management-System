using Application.Abstraction;
using Application.Contracts.ReportsStatistics;
using Domain.Entities;

namespace Application.Service.ReportsStatistics;

public interface IReportsStatisticsService
{
    Task<Result<ReportsStatisticsDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemReportDefinitionResponse>>> GetDefinitionsAsync(SystemReportKind? kind = null, CancellationToken cancellationToken = default);
    Task<Result<SystemReportDefinitionResponse>> SaveDefinitionAsync(int? id, SaveSystemReportDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<Result<SystemReportRunResponse>> GenerateReportAsync(GenerateSystemReportRequest request, CancellationToken cancellationToken = default);
    Task<Result<SystemReportExportResponse>> ExportReportAsync(GenerateSystemReportRequest request, CancellationToken cancellationToken = default);
    Task<Result<SystemReportExportResponse>> GetArchivedExportAsync(int runId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemReportRunResponse>>> GetRunsAsync(string? reportKey = null, bool includeArchived = false, CancellationToken cancellationToken = default);
    Task<Result<ArchiveSystemReportRunsResponse>> ArchiveRunsAsync(ArchiveSystemReportRunsRequest request, CancellationToken cancellationToken = default);
}
