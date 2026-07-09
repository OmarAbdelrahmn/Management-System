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
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateSystemReportRequest request, CancellationToken ct) => ToAction(await service.GenerateReportAsync(request, ct));
    [HttpGet("runs")]
    public async Task<IActionResult> Runs([FromQuery] string? reportKey, CancellationToken ct) => ToAction(await service.GetRunsAsync(reportKey, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
