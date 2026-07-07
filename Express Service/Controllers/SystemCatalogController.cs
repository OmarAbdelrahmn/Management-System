using Application.Contracts.SystemCatalog;
using Application.Service.SystemCatalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/system-catalog")]
[ApiController]
[Authorize(Roles = "Admin")]
public class SystemCatalogController(ISystemCatalogService service) : ControllerBase
{
    [HttpPost("seed-first-pages")]
    public async Task<IActionResult> SeedFirstPages(CancellationToken cancellationToken)
    {
        var result = await service.SeedFirstPagesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("modules")]
    public async Task<IActionResult> Modules(CancellationToken cancellationToken)
    {
        var result = await service.GetModulesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("pages")]
    public async Task<IActionResult> Pages([FromQuery] string? moduleKey, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await service.GetPagesAsync(moduleKey, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("pages/{id:int}")]
    public async Task<IActionResult> Page(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetPageAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("pages/{id:int}/status")]
    public async Task<IActionResult> Status(int id, [FromBody] UpdateSystemPageStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdatePageStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
