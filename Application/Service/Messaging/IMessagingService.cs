using Application.Abstraction;
using Application.Contracts.Emails;
using Application.Contracts.Messaging;
using Domain.Entities;

namespace Application.Service.Messaging;

public interface IMessagingService
{
    Task<Result<IEnumerable<InternalMailResponse>>> GetInboxAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<InternalMailResponse>>> GetSentAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<InternalMailResponse>>> GetArchivedAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<InternalMailResponse>>> GetDraftsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<InternalMailResponse>> CreateMailAsync(CreateInternalMailRequest request, CancellationToken cancellationToken = default);
    Task<Result<InternalMailResponse>> UpdateDraftAsync(int id, UpdateInternalMailDraftRequest request, CancellationToken cancellationToken = default);
    Task<Result> SendDraftAsync(int id, SendDraftRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelDraftAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> MarkMailReadAsync(int recipientId, CancellationToken cancellationToken = default);
    Task<Result> ArchiveMailRecipientAsync(int recipientId, bool isArchived, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MessageTemplateResponse>>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<Result<MessageTemplateResponse>> SaveTemplateAsync(int? id, UpsertMessageTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NotificationResponse>>> GetNotificationsAsync(NotificationStatus? status = null, MessageChannel? channel = null, string? keyword = null, CancellationToken cancellationToken = default);
    Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<Result<NotificationResponse>> UpdateScheduledNotificationAsync(int id, UpdateScheduledNotificationRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelNotificationAsync(int id, CancelNotificationRequest request, CancellationToken cancellationToken = default);
    Task<Result<NotificationRecipientResponse>> MarkNotificationRecipientReadAsync(int recipientId, CancellationToken cancellationToken = default);
    Task<Result<NotificationRecipientResponse>> RecordNotificationDeliveryAsync(int recipientId, UpdateNotificationDeliveryRequest request, CancellationToken cancellationToken = default);
    Task<Result<NotificationRecipientResponse>> RetryNotificationRecipientAsync(int recipientId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ChannelDeliveryLogResponse>>> GetChannelLogsAsync(MessageChannel? channel = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EmailOutboxResponse>>> GetEmailOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default);
}
