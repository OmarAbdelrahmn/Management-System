using Application.Contracts.Boards;
using Application.Service.Boards;
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
    [Authorize(Roles = "Admin,BoardSecretary")]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("close-expired")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CloseExpired(CancellationToken cancellationToken)
    {
        var result = await service.CloseExpiredBoardsAsync(DateTime.UtcNow.AddHours(3), cancellationToken);
        return result.IsSuccess ? Ok(new { ClosedCount = result.Value }) : result.ToProblem();
    }
}
