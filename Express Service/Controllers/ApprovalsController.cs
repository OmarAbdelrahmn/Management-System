using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApprovalsController(ITaskManagementService taskService) : ControllerBase
{
    [HttpGet("routes")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> Routes(CancellationToken cancellationToken)
    {
        var result = await taskService.GetApprovalRoutesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("routes")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateApprovalRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateApprovalRouteAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("routes/{id:int}")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateApprovalRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.UpdateApprovalRouteAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("routes/{id:int}/steps")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> AddStep(int id, [FromBody] AddApprovalStepRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.AddApprovalStepAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("routes/{routeId:int}/steps/{stepId:int}")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> UpdateStep(int routeId, int stepId, [FromBody] UpdateApprovalStepRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.UpdateApprovalStepAsync(routeId, stepId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending([FromQuery] bool mineOnly, CancellationToken cancellationToken)
    {
        var userId = mineOnly ? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") : null;
        var result = await taskService.GetPendingApprovalRequestsAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("requests")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateApprovalRequestAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("requests/{id:int}/decide")]
    public async Task<IActionResult> Decide(int id, [FromBody] DecideApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await taskService.DecideApprovalRequestAsync(id, request with { ActionByUserId = actorUserId }, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("requests/{id:int}/delegate")]
    public async Task<IActionResult> Delegate(int id, [FromBody] DelegateApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await taskService.DelegateApprovalRequestAsync(id, request with { ActionByUserId = actorUserId }, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("requests/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await taskService.CancelApprovalRequestAsync(id, request with { RequestedByUserId = actorUserId }, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
