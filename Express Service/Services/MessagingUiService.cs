using Application.Contracts.Emails;
using Application.Contracts.Messaging;
using Application.Contracts.TaskManagement;
using Application.Service.Messaging;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Express_Service.Services;

public class MessagingUiService(
    IMessagingService messagingService,
    ApplicationDbcontext dbcontext,
    IHttpContextAccessor httpContextAccessor)
{
    public string? CurrentUserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<List<UserPickerResponse>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        await dbcontext.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new UserPickerResponse(x.Id, x.FullName, x.Email))
            .ToListAsync(cancellationToken);

    public async Task<List<InternalMailResponse>> GetInboxAsync(CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetInboxAsync(CurrentUserId ?? string.Empty, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<InternalMailResponse>> GetDraftsAsync(CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetDraftsAsync(CurrentUserId ?? string.Empty, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<InternalMailResponse>> GetSentAsync(CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetSentAsync(CurrentUserId ?? string.Empty, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<InternalMailResponse>> GetArchivedAsync(CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetArchivedAsync(CurrentUserId ?? string.Empty, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateMailAsync(CreateInternalMailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.CreateMailAsync(request, cancellationToken);
        return result.IsSuccess ? (true, request.SendNow ? "تم إرسال الرسالة." : "تم حفظ المسودة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SendDraftAsync(int id, IReadOnlyList<string> recipientUserIds, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.SendDraftAsync(id, new SendDraftRequest(recipientUserIds), cancellationToken);
        return result.IsSuccess ? (true, "تم إرسال المسودة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateDraftAsync(int id, UpdateInternalMailDraftRequest request, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.UpdateDraftAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث المسودة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelDraftAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.CancelDraftAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تم حذف المسودة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> MarkMailReadAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.MarkMailReadAsync(recipientId, cancellationToken);
        return result.IsSuccess ? (true, "تم تعليم الرسالة كمقروءة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> ArchiveMailRecipientAsync(int recipientId, bool isArchived, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.ArchiveMailRecipientAsync(recipientId, isArchived, cancellationToken);
        return result.IsSuccess
            ? (true, isArchived ? "تمت أرشفة الرسالة." : "تمت استعادة الرسالة.")
            : (false, result.Error.Description);
    }

    public async Task<List<MessageTemplateResponse>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetTemplatesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveTemplateAsync(int? id, UpsertMessageTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.SaveTemplateAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ القالب.") : (false, result.Error.Description);
    }

    public async Task<List<NotificationResponse>> GetNotificationsAsync(NotificationStatus? status = null, MessageChannel? channel = null, string? keyword = null, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetNotificationsAsync(status, channel, keyword, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.CreateNotificationAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء الإشعار.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelNotificationAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.CancelNotificationAsync(id, new CancelNotificationRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء الإشعار.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> MarkNotificationRecipientReadAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.MarkNotificationRecipientReadAsync(recipientId, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة القراءة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RecordNotificationDeliveryAsync(int recipientId, UpdateNotificationDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.RecordNotificationDeliveryAsync(recipientId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة التسليم.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RetryNotificationRecipientAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.RetryNotificationRecipientAsync(recipientId, cancellationToken);
        return result.IsSuccess ? (true, "تمت إعادة جدولة الإرسال للمستلم.") : (false, result.Error.Description);
    }

    public async Task<List<ChannelDeliveryLogResponse>> GetChannelLogsAsync(MessageChannel? channel = null, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetChannelLogsAsync(channel, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<EmailOutboxResponse>> GetEmailOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default)
    {
        var result = await messagingService.GetEmailOutboxAsync(sent, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }
}
