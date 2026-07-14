using Application.Contracts.Messaging;
using Application.Service.Messaging;
using Domain.Entities;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController(IMessagingService messagingService) : ControllerBase
{
    [HttpGet]
    [RequirePermission("system.executive-supervision.notifications_database")]
    public async Task<IActionResult> Search([FromQuery] NotificationStatus? status, [FromQuery] MessageChannel? channel, [FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetNotificationsAsync(status, channel, keyword, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [RequirePermission("system.executive-supervision.notifications_management")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.CreateNotificationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}/scheduled")]
    [RequirePermission("system.executive-supervision.notifications_management")]
    public async Task<IActionResult> UpdateScheduled(int id, [FromBody] UpdateScheduledNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.UpdateScheduledNotificationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/cancel")]
    [RequirePermission("system.executive-supervision.notifications_delete")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.CancelNotificationAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/read")]
    [RequirePermission("system.executive-supervision.notifications_database")]
    public async Task<IActionResult> MarkRecipientRead(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.MarkNotificationRecipientReadAsync(recipientId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/delivery")]
    [RequirePermission("system.executive-supervision.notifications_management")]
    public async Task<IActionResult> RecordRecipientDelivery(int recipientId, [FromBody] UpdateNotificationDeliveryRequest request, CancellationToken cancellationToken)
    {
        var result = await messagingService.RecordNotificationDeliveryAsync(recipientId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recipients/{recipientId:int}/retry")]
    [RequirePermission("system.executive-supervision.notifications_management")]
    public async Task<IActionResult> RetryRecipient(int recipientId, CancellationToken cancellationToken)
    {
        var result = await messagingService.RetryNotificationRecipientAsync(recipientId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("channel-logs")]
    [RequirePermission("system.executive-supervision.notifications_database")]
    public async Task<IActionResult> ChannelLogs([FromQuery] MessageChannel? channel, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetChannelLogsAsync(channel, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("email-outbox")]
    [RequirePermission("system.tech-enablement.system_channel_records_email")]
    public async Task<IActionResult> EmailOutbox([FromQuery] bool? sent, CancellationToken cancellationToken)
    {
        var result = await messagingService.GetEmailOutboxAsync(sent, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
