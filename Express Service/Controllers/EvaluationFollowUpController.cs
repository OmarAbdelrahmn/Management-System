using Application.Contracts.EvaluationFollowUp;
using Application.Service.EvaluationFollowUp;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequirePermissionPrefix("system.evaluation-followup.")]
public class EvaluationFollowUpController(IEvaluationFollowUpService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("cases")]
    public async Task<IActionResult> Cases([FromQuery] FollowUpCaseStatus? status, [FromQuery] FollowUpSubjectType? subjectType, CancellationToken ct) => ToAction(await service.GetCasesAsync(status, subjectType, ct));
    [HttpPost("cases")]
    public async Task<IActionResult> SaveCase([FromBody] SaveFollowUpCaseRequest request, CancellationToken ct) => ToAction(await service.SaveCaseAsync(null, request, ct));
    [HttpPut("cases/{id:int}")]
    public async Task<IActionResult> UpdateCase(int id, [FromBody] SaveFollowUpCaseRequest request, CancellationToken ct) => ToAction(await service.SaveCaseAsync(id, request, ct));
    [HttpPost("cases/{id:int}/status")]
    public async Task<IActionResult> UpdateCaseStatus(int id, [FromBody] UpdateFollowUpCaseStatusRequest request, CancellationToken ct) => ToAction(await service.UpdateCaseStatusAsync(id, request, ct));
    [HttpGet("activities")]
    public async Task<IActionResult> Activities([FromQuery] int? caseId, [FromQuery] FollowUpSubjectType? subjectType, [FromQuery] bool nextActionsOnly, CancellationToken ct) => ToAction(await service.GetActivitiesAsync(caseId, subjectType, nextActionsOnly, ct));
    [HttpPost("activities")]
    public async Task<IActionResult> SaveActivity([FromBody] SaveFollowUpActivityRequest request, CancellationToken ct) => ToAction(await service.SaveActivityAsync(null, request, ct));
    [HttpPut("activities/{id:int}")]
    public async Task<IActionResult> UpdateActivity(int id, [FromBody] SaveFollowUpActivityRequest request, CancellationToken ct) => ToAction(await service.SaveActivityAsync(id, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
