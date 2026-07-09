using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
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
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Routes(CancellationToken cancellationToken)
    {
        var result = await taskService.GetApprovalRoutesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("routes")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateApprovalRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateApprovalRouteAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("routes/{id:int}/steps")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> AddStep(int id, [FromBody] AddApprovalStepRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.AddApprovalStepAsync(id, request, cancellationToken);
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
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateApprovalRequestAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("requests/{id:int}/decide")]
    public async Task<IActionResult> Decide(int id, [FromBody] DecideApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.DecideApprovalRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
