using Application.Contracts.DocumentationArchive;
using Application.Service.DocumentationArchive;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class DocumentationArchiveController(IDocumentationArchiveService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("documents")]
    public async Task<IActionResult> Documents([FromQuery] ArchiveDocumentCategory? category, [FromQuery] ArchiveDocumentStatus? status, CancellationToken ct) => ToAction(await service.GetArchiveDocumentsAsync(category, status, ct));
    [HttpPost("documents")]
    public async Task<IActionResult> SaveDocument([FromBody] SaveArchiveDocumentRequest request, CancellationToken ct) => ToAction(await service.SaveArchiveDocumentAsync(null, request, ct));
    [HttpGet("correspondence")]
    public async Task<IActionResult> Correspondence([FromQuery] CorrespondenceDirection? direction, [FromQuery] CorrespondenceStatus? status, CancellationToken ct) => ToAction(await service.GetCorrespondenceAsync(direction, status, ct));
    [HttpPost("correspondence")]
    public async Task<IActionResult> SaveCorrespondence([FromBody] SaveCorrespondenceRecordRequest request, CancellationToken ct) => ToAction(await service.SaveCorrespondenceAsync(null, request, ct));
    [HttpPatch("correspondence/{id:int}/status")]
    public async Task<IActionResult> UpdateCorrespondenceStatus(int id, [FromBody] UpdateCorrespondenceStatusRequest request, CancellationToken ct)
    {
        var result = await service.UpdateCorrespondenceStatusAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("operations")]
    public async Task<IActionResult> Operations([FromQuery] CorrespondenceOperationStatus? status, [FromQuery] int? correspondenceRecordId, CancellationToken ct) => ToAction(await service.GetOperationsAsync(status, correspondenceRecordId, ct));
    [HttpPost("operations")]
    public async Task<IActionResult> SaveOperation([FromBody] SaveCorrespondenceOperationRequest request, CancellationToken ct) => ToAction(await service.SaveOperationAsync(null, request, ct));
    [HttpPatch("operations/{id:int}/complete")]
    public async Task<IActionResult> CompleteOperation(int id, [FromBody] CompleteCorrespondenceOperationRequest request, CancellationToken ct)
    {
        var result = await service.CompleteOperationAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
