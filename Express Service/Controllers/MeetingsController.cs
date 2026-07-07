using Application.Contracts.Meetings;
using Application.Service.Meetings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeetingsController(IMeetingService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("scheduled")]
    public async Task<IActionResult> Scheduled(CancellationToken cancellationToken)
    {
        var result = await service.GetScheduledAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> Calendar(CancellationToken cancellationToken)
    {
        var result = await service.GetCalendarAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("repeated-drafts")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> RepeatedDrafts(CancellationToken cancellationToken)
    {
        var result = await service.GetRepeatedDraftsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("archive")]
    public async Task<IActionResult> Archive([FromQuery] string? type, CancellationToken cancellationToken)
    {
        var result = await service.GetArchiveAsync(type, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Create([FromBody] CreateMeetingRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/agenda-items")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> AddAgendaItem(int id, [FromBody] AddAgendaItemRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AddAgendaItemAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/submit-for-approval")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> SubmitForApproval(int id, [FromBody] SubmitMeetingApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SubmitForApprovalAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Approve(int id, [FromBody] DecideMeetingApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await service.ApproveAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Reject(int id, [FromBody] DecideMeetingApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RejectAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Start(int id, CancellationToken cancellationToken)
    {
        var result = await service.StartAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/finish")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Finish(int id, CancellationToken cancellationToken)
    {
        var result = await service.FinishAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
