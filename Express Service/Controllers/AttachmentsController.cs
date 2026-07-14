using Application.Contracts.Attachments;
using Application.Service.Attachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/attachments")]
[ApiController]
[Authorize]
public class AttachmentsController(IAttachmentService service, IAttachmentStorage storage) : ControllerBase
{
    [HttpGet("access")]
    public async Task<IActionResult> Access([FromQuery] string entityType, [FromQuery] string entityId, CancellationToken cancellationToken)
    {
        var result = await service.GetEntityAccessAsync(entityType, entityId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? file, [FromForm] string? category, [FromForm] string? entityType, [FromForm] string? entityId, [FromForm] string? label, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("A file is required.");
        await using var stream = file.OpenReadStream();
        var result = await service.UploadAsync(new AttachmentUploadInput(stream, file.FileName, file.ContentType, file.Length, category ?? "General", entityType, entityId, label), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/versions")]
    public async Task<IActionResult> AddVersion(int id, IFormFile? file, [FromForm] string? category, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("A file is required.");
        await using var stream = file.OpenReadStream();
        var result = await service.AddVersionAsync(id, new AttachmentUploadInput(stream, file.FileName, file.ContentType, file.Length, category ?? "General", null, null, null), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string entityType, [FromQuery] string entityId, CancellationToken cancellationToken)
    {
        var result = await service.GetForEntityAsync(entityType, entityId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}/versions")]
    public async Task<IActionResult> Versions(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetVersionsAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetDownloadAsync(id, cancellationToken);
        if (!result.IsSuccess) return result.ToProblem();
        var stream = await storage.OpenReadAsync(result.Value.Path, cancellationToken);
        return stream is null ? NotFound() : File(stream, result.Value.ContentType, result.Value.DownloadName);
    }

    [HttpGet("{id:int}/preview")]
    public async Task<IActionResult> Preview(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetDownloadAsync(id, cancellationToken);
        if (!result.IsSuccess) return result.ToProblem();
        if (!IsPreviewable(result.Value.ContentType)) return BadRequest("This file type cannot be previewed safely.");

        var stream = await storage.OpenReadAsync(result.Value.Path, cancellationToken);
        if (stream is null) return NotFound();

        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        Response.Headers.Append("Content-Security-Policy", "sandbox");
        return new FileStreamResult(stream, result.Value.ContentType) { EnableRangeProcessing = true };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("links/{linkId:int}")]
    public async Task<IActionResult> Unlink(int linkId, CancellationToken cancellationToken)
    {
        var result = await service.UnlinkAsync(linkId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    private static bool IsPreviewable(string contentType) =>
        contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        contentType.StartsWith("image/png", StringComparison.OrdinalIgnoreCase) ||
        contentType.StartsWith("image/jpeg", StringComparison.OrdinalIgnoreCase);
}
