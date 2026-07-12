using Application.Contracts.ReportsStatistics;
using Application.Service.ReportsStatistics;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class ReportsStatisticsController(IReportsStatisticsService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("definitions")]
    public async Task<IActionResult> Definitions([FromQuery] SystemReportKind? kind, CancellationToken ct) => ToAction(await service.GetDefinitionsAsync(kind, ct));
    [HttpPost("definitions")]
    public async Task<IActionResult> SaveDefinition([FromBody] SaveSystemReportDefinitionRequest request, CancellationToken ct) => ToAction(await service.SaveDefinitionAsync(null, request, ct));
    [HttpPut("definitions/{id:int}")]
    public async Task<IActionResult> UpdateDefinition(int id, [FromBody] SaveSystemReportDefinitionRequest request, CancellationToken ct) => ToAction(await service.SaveDefinitionAsync(id, request, ct));
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateSystemReportRequest request, CancellationToken ct) => ToAction(await service.GenerateReportAsync(request, ct));
    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] GenerateSystemReportRequest request, CancellationToken ct) => ToAction(await service.ExportReportAsync(request, ct));
    [HttpGet("runs")]
    public async Task<IActionResult> Runs([FromQuery] string? reportKey, [FromQuery] bool includeArchived, CancellationToken ct) => ToAction(await service.GetRunsAsync(reportKey, includeArchived, ct));
    [HttpPost("runs/archive")]
    public async Task<IActionResult> ArchiveRuns([FromBody] ArchiveSystemReportRunsRequest request, CancellationToken ct) => ToAction(await service.ArchiveRunsAsync(request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
