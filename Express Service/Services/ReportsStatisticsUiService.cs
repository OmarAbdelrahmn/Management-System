using Application.Contracts.ReportsStatistics;
using Application.Service.ReportsStatistics;
using Domain.Entities;

namespace Express_Service.Services;

public class ReportsStatisticsUiService(IReportsStatisticsService service)
{
    public async Task<ReportsStatisticsDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<SystemReportDefinitionResponse>> GetDefinitionsAsync(SystemReportKind? kind = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetDefinitionsAsync(kind, cancellationToken));

    public async Task<(bool Success, string Message)> SaveDefinitionAsync(int? id, SaveSystemReportDefinitionRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveDefinitionAsync(id, request, cancellationToken), "تم حفظ تعريف التقرير.");

    public async Task<(bool Success, string Message)> GenerateReportAsync(GenerateSystemReportRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.GenerateReportAsync(request, cancellationToken), "تم توليد التقرير.");

    public async Task<List<SystemReportRunResponse>> GetRunsAsync(string? reportKey = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetRunsAsync(reportKey, cancellationToken));

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
