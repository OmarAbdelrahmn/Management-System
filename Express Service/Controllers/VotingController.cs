using Application.Contracts.Voting;
using Application.Service.Voting;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VotingController(IVotingService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("agenda-items/{agendaItemId:int}/open")]
    [RequirePermission("system.documentation-archive.meetings_management")]
    public async Task<IActionResult> Open(int agendaItemId, CancellationToken cancellationToken)
    {
        var result = await service.OpenAsync(agendaItemId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/votes")]
    [RequirePermissionPrefix("system.documentation-archive.meetings_")]
    public async Task<IActionResult> CastVote(int id, [FromBody] CastVoteRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CastVoteAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/close")]
    [RequirePermission("system.documentation-archive.meetings_management")]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
    {
        var result = await service.CloseAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
