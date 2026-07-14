using Application.Contracts.Boards;
using Application.Service.Boards;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BoardsController(IBoardService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [RequirePermissionPrefix("system.participating-members.")]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    [RequirePermissionPrefix("system.participating-members.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}/members")]
    [RequirePermissionPrefix("system.participating-members.")]
    public async Task<IActionResult> SaveMembers(int id, [FromBody] SaveBoardMembersRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveMembersAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/renew")]
    [RequirePermissionPrefix("system.participating-members.")]
    public async Task<IActionResult> RenewCycle(int id, [FromBody] RenewBoardCycleRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RenewCycleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("close-expired")]
    [RequirePermission("system.participating-members.hr_board_create")]
    public async Task<IActionResult> CloseExpired(CancellationToken cancellationToken)
    {
        var result = await service.CloseExpiredBoardsAsync(DateTime.UtcNow.AddHours(3), cancellationToken);
        return result.IsSuccess ? Ok(new { ClosedCount = result.Value }) : result.ToProblem();
    }
}
