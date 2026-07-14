using Application.Contracts.Messaging;
using Application.Service.Messaging;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MailController(IMessagingService messagingService) : ControllerBase
{
    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await messagingService.GetInboxAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("drafts")]
    public async Task<IActionResult> Drafts(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await messagingService.GetDraftsAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sent")]
    public async Task<IActionResult> Sent(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await messagingService.GetSentAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("archived")]
    public async Task<IActionResult> Archived(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await messagingService.GetArchivedAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [RequirePermission("system.electronic-office.personnel_emails_drafts")]
    public async Task<IActionResult> Create([FromBody] CreateInternalMailRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.CreateMailAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/send")]
    [RequirePermission("system.electronic-office.personnel_emails_drafts")]
    public async Task<IActionResult> SendDraft(int id, [FromBody] SendDraftRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.SendDraftAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("drafts/{id:int}")]
    [RequirePermission("system.electronic-office.personnel_emails_drafts")]
    public async Task<IActionResult> UpdateDraft(int id, [FromBody] UpdateInternalMailDraftRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.UpdateDraftAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("drafts/{id:int}")]
    [RequirePermission("system.electronic-office.personnel_emails_drafts")]
    public async Task<IActionResult> CancelDraft(int id, CancellationToken cancellationToken)
    {
        var result = await messagingService.CancelDraftAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/read")]
    public async Task<IActionResult> MarkRead(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.MarkMailReadAsync(recipientId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/archive")]
    public async Task<IActionResult> ArchiveRecipient(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.ArchiveMailRecipientAsync(recipientId, true, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/restore")]
    public async Task<IActionResult> RestoreRecipient(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.ArchiveMailRecipientAsync(recipientId, false, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("templates")]
    [RequirePermission("system.electronic-office.personnel_emails_templates")]
    public async Task<IActionResult> Templates(CancellationToken cancellationToken)
    {
        var result = await messagingService.GetTemplatesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("templates")]
    [RequirePermission("system.electronic-office.personnel_emails_templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] UpsertMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.SaveTemplateAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("templates/{id:int}")]
    [RequirePermission("system.electronic-office.personnel_emails_templates")]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpsertMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.SaveTemplateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
