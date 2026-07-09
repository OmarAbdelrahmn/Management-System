using Application.Contracts.Messaging;
using Application.Service.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController(IMessagingService messagingService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Search([FromQuery] NotificationStatus? status, [FromQuery] MessageChannel? channel, [FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetNotificationsAsync(status, channel, keyword, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.CreateNotificationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.CancelNotificationAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/read")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> MarkRecipientRead(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.MarkNotificationRecipientReadAsync(recipientId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/delivery")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> RecordRecipientDelivery(int recipientId, [FromBody] UpdateNotificationDeliveryRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.RecordNotificationDeliveryAsync(recipientId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/retry")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> RetryRecipient(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.RetryNotificationRecipientAsync(recipientId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("channel-logs")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> ChannelLogs([FromQuery] MessageChannel? channel, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetChannelLogsAsync(channel, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("email-outbox")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> EmailOutbox([FromQuery] bool? sent, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetEmailOutboxAsync(sent, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
