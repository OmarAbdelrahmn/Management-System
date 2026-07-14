using Application.Contracts.Minutes;
using Application.Service.Minutes;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MinutesController(IMinuteService service) : ControllerBase
{
    [HttpGet("archive")]
    public async Task<IActionResult> Archive(CancellationToken cancellationToken)
    {
        var result = await service.GetArchiveAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("meeting/{meetingId:int}")]
    public async Task<IActionResult> Get(int meetingId, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(meetingId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("meeting/{meetingId:int}/sign")]
    [RequirePermission("system.documentation-archive.meetings_mom_approve")]
    public async Task<IActionResult> Sign(int meetingId, [FromBody] SignMinuteRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SignAndPublishAsync(meetingId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("meeting/{meetingId:int}/approve")]
    [RequirePermission("system.documentation-archive.meetings_mom_approve")]
    public async Task<IActionResult> Approve(int meetingId, CancellationToken cancellationToken)
    {
        var chairmanUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var result = await service.SignAndPublishAsync(meetingId, new SignMinuteRequest(chairmanUserId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("meeting/{meetingId:int}/cancel-approval")]
    [RequirePermission("system.documentation-archive.meetings_approved")]
    public async Task<IActionResult> CancelApproval(int meetingId, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var result = await service.CancelApprovalAsync(meetingId, actorUserId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("meeting/{meetingId:int}/pdf")]
    public async Task<IActionResult> Pdf(int meetingId, CancellationToken cancellationToken)
    {
        var result = await service.GeneratePdfAsync(meetingId, cancellationToken);
        return result.IsSuccess
            ? File(result.Value, "application/pdf", $"meeting-minute-{meetingId}.pdf")
            : result.ToProblem();
    }
}
