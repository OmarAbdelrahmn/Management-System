using Application.Contracts.Invitations;
using Application.Service.Invitations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvitationsController(IInvitationService service) : ControllerBase
{
    [HttpGet("meeting/{meetingId:int}")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> GetMeetingInvitations(int meetingId, CancellationToken cancellationToken)
    {
        var result = await service.GetMeetingInvitationsAsync(meetingId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("meeting/{meetingId:int}/send")]
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Send(int meetingId, CancellationToken cancellationToken)
    {
        var result = await service.SendMeetingInvitationsAsync(meetingId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/accept")]
    [Authorize(Roles = "BoardMember,BoardChairman,Admin")]
    public async Task<IActionResult> Accept(int id, [FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AcceptAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
