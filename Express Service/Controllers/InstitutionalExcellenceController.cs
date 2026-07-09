using Application.Contracts.InstitutionalExcellence;
using Application.Service.InstitutionalExcellence;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class InstitutionalExcellenceController(IInstitutionalExcellenceService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("performance-measures")]
    public async Task<IActionResult> PerformanceMeasures([FromQuery] ExcellenceRecordStatus? status, CancellationToken ct) => ToAction(await service.GetPerformanceMeasuresAsync(status, ct));
    [HttpPost("performance-measures")]
    public async Task<IActionResult> SavePerformanceMeasure([FromBody] SavePerformanceMeasureRequest request, CancellationToken ct) => ToAction(await service.SavePerformanceMeasureAsync(null, request, ct));
    [HttpGet("governance-cycles")]
    public async Task<IActionResult> GovernanceCycles([FromQuery] ExcellenceRecordStatus? status, CancellationToken ct) => ToAction(await service.GetGovernanceCyclesAsync(status, ct));
    [HttpPost("governance-cycles")]
    public async Task<IActionResult> SaveGovernanceCycle([FromBody] SaveGovernanceCycleRequest request, CancellationToken ct) => ToAction(await service.SaveGovernanceCycleAsync(null, request, ct));
    [HttpGet("governance-criteria")]
    public async Task<IActionResult> GovernanceCriteria([FromQuery] int? cycleId, [FromQuery] GovernanceCriterionStatus? status, CancellationToken ct) => ToAction(await service.GetGovernanceCriteriaAsync(cycleId, status, ct));
    [HttpPost("governance-criteria")]
    public async Task<IActionResult> SaveGovernanceCriterion([FromBody] SaveGovernanceCriterionRequest request, CancellationToken ct) => ToAction(await service.SaveGovernanceCriterionAsync(null, request, ct));
    [HttpGet("governance-attachments")]
    public async Task<IActionResult> GovernanceAttachments([FromQuery] int? criterionId, CancellationToken ct) => ToAction(await service.GetGovernanceAttachmentsAsync(criterionId, ct));
    [HttpPost("governance-attachments")]
    public async Task<IActionResult> SaveGovernanceAttachment([FromBody] SaveGovernanceAttachmentRequest request, CancellationToken ct) => ToAction(await service.SaveGovernanceAttachmentAsync(null, request, ct));
    [HttpGet("governance-tasks")]
    public async Task<IActionResult> GovernanceTasks([FromQuery] int? cycleId, [FromQuery] GovernanceTaskStatus? status, CancellationToken ct) => ToAction(await service.GetGovernanceTasksAsync(cycleId, status, ct));
    [HttpPost("governance-tasks")]
    public async Task<IActionResult> SaveGovernanceTask([FromBody] SaveGovernanceTaskRequest request, CancellationToken ct) => ToAction(await service.SaveGovernanceTaskAsync(null, request, ct));
    [HttpGet("governance-report")]
    public async Task<IActionResult> GovernanceReport([FromQuery] int? cycleId, CancellationToken ct) => ToAction(await service.GetGovernanceReportAsync(cycleId, ct));
    [HttpGet("strategic-plans")]
    public async Task<IActionResult> StrategicPlans([FromQuery] ExcellenceRecordStatus? status, CancellationToken ct) => ToAction(await service.GetStrategicPlansAsync(status, ct));
    [HttpPost("strategic-plans")]
    public async Task<IActionResult> SaveStrategicPlan([FromBody] SaveStrategicPlanRequest request, CancellationToken ct) => ToAction(await service.SaveStrategicPlanAsync(null, request, ct));
    [HttpGet("strategic-perspectives")]
    public async Task<IActionResult> StrategicPerspectives([FromQuery] int? planId, CancellationToken ct) => ToAction(await service.GetStrategicPerspectivesAsync(planId, ct));
    [HttpPost("strategic-perspectives")]
    public async Task<IActionResult> SaveStrategicPerspective([FromBody] SaveStrategicPerspectiveRequest request, CancellationToken ct) => ToAction(await service.SaveStrategicPerspectiveAsync(null, request, ct));
    [HttpGet("strategic-goals")]
    public async Task<IActionResult> StrategicGoals([FromQuery] int? perspectiveId, CancellationToken ct) => ToAction(await service.GetStrategicGoalsAsync(perspectiveId, ct));
    [HttpPost("strategic-goals")]
    public async Task<IActionResult> SaveStrategicGoal([FromBody] SaveStrategicGoalRequest request, CancellationToken ct) => ToAction(await service.SaveStrategicGoalAsync(null, request, ct));
    [HttpGet("strategic-indicators")]
    public async Task<IActionResult> StrategicIndicators([FromQuery] int? planId, [FromQuery] StrategicIndicatorKind? kind, CancellationToken ct) => ToAction(await service.GetStrategicIndicatorsAsync(planId, kind, ct));
    [HttpPost("strategic-indicators")]
    public async Task<IActionResult> SaveStrategicIndicator([FromBody] SaveStrategicIndicatorRequest request, CancellationToken ct) => ToAction(await service.SaveStrategicIndicatorAsync(null, request, ct));
    [HttpGet("strategic-variables")]
    public async Task<IActionResult> StrategicVariables([FromQuery] int? planId, CancellationToken ct) => ToAction(await service.GetStrategicVariablesAsync(planId, ct));
    [HttpPost("strategic-variables")]
    public async Task<IActionResult> SaveStrategicVariable([FromBody] SaveStrategicVariableRequest request, CancellationToken ct) => ToAction(await service.SaveStrategicVariableAsync(null, request, ct));
    [HttpPost("strategic-plans/{planId:int}/fetch-variables")]
    public async Task<IActionResult> FetchVariables(int planId, CancellationToken ct) => ToAction(await service.FetchAutomatedStrategicVariablesAsync(planId, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
