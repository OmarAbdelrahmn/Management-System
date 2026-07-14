using Application.Contracts.ReportsStatistics;
using Application.Service.ReportsStatistics;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequirePermissionPrefix("system.reports-statistics.")]
public class ReportsStatisticsController(IReportsStatisticsService service, IAuthorizationService authorization) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("definitions")]
    public async Task<IActionResult> Definitions([FromQuery] SystemReportKind? kind, CancellationToken ct) => ToAction(await service.GetDefinitionsAsync(kind, ct));
    [HttpPost("definitions")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> SaveDefinition([FromBody] SaveSystemReportDefinitionRequest request, CancellationToken ct) => ToAction(await service.SaveDefinitionAsync(null, request, ct));
    [HttpPut("definitions/{id:int}")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> UpdateDefinition(int id, [FromBody] SaveSystemReportDefinitionRequest request, CancellationToken ct) => ToAction(await service.SaveDefinitionAsync(id, request, ct));
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateSystemReportRequest request, CancellationToken ct)
    {
        if (!await CanAccessReportAsync(request.ReportKey)) return Forbid();
        return ToAction(await service.GenerateReportAsync(request, ct));
    }
    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] GenerateSystemReportRequest request, CancellationToken ct)
    {
        if (!await CanAccessReportAsync(request.ReportKey)) return Forbid();
        var result = await service.ExportReportAsync(request, ct);
        return !result.IsSuccess ? result.ToProblem() : result.Value.BinaryContent is not null
            ? File(result.Value.BinaryContent, result.Value.ContentType, result.Value.FileName)
            : Ok(result.Value);
    }
    [HttpGet("runs/{id:int}/archive")]
    public async Task<IActionResult> DownloadArchive(int id, CancellationToken ct)
    {
        var result = await service.GetArchivedExportAsync(id, ct);
        if (!result.IsSuccess) return result.ToProblem();
        if (!await CanAccessReportAsync(result.Value.Run.ReportKey)) return Forbid();
        return File(result.Value.BinaryContent ?? System.Text.Encoding.UTF8.GetBytes(result.Value.Content), result.Value.ContentType, result.Value.FileName);
    }
    [HttpGet("runs")]
    public async Task<IActionResult> Runs([FromQuery] string? reportKey, [FromQuery] bool includeArchived, CancellationToken ct) => ToAction(await service.GetRunsAsync(reportKey, includeArchived, ct));
    [HttpPost("runs/archive")]
    public async Task<IActionResult> ArchiveRuns([FromBody] ArchiveSystemReportRunsRequest request, CancellationToken ct) => ToAction(await service.ArchiveRunsAsync(request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    private async Task<bool> CanAccessReportAsync(string? reportKey)
    {
        if (string.IsNullOrWhiteSpace(reportKey)) return false;
        var result = await authorization.AuthorizeAsync(User, HttpContext, $"permission:system.reports-statistics.{reportKey.Trim()}");
        return result.Succeeded;
    }
}
