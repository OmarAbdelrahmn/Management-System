using Domain.Entities;

namespace Application.Contracts.Messaging;

public record InternalMailResponse(
    int Id,
    string SenderUserId,
    string SenderName,
    string Subject,
    string Body,
    string Status,
    DateTime? SentAt,
    DateTime CreatedAt,
    IReadOnlyList<InternalMailRecipientResponse> Recipients);

public record InternalMailRecipientResponse(
    int Id,
    string RecipientUserId,
    string RecipientName,
    bool IsRead,
    DateTime? ReadAt,
    bool IsArchived);

public record CreateInternalMailRequest(
    string Subject,
    string Body,
    IReadOnlyList<string> RecipientUserIds,
    bool SendNow);

public record SendDraftRequest(IReadOnlyList<string> RecipientUserIds);

public record UpdateInternalMailDraftRequest(
    string Subject,
    string Body,
    IReadOnlyList<string> RecipientUserIds);

public record MessageTemplateResponse(
    int Id,
    string Key,
    string NameAr,
    string Subject,
    string Body,
    string Channel,
    bool IsActive);

public record UpsertMessageTemplateRequest(
    string Key,
    string NameAr,
    string Subject,
    string Body,
    MessageChannel Channel,
    bool IsActive);

public record NotificationResponse(
    int Id,
    string Title,
    string Body,
    string Channel,
    string Status,
    string CreatedBySystemUserId,
    string CreatedByName,
    DateTime? ScheduledAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt,
    IReadOnlyList<NotificationRecipientResponse> Recipients);

public record NotificationRecipientResponse(
    int Id,
    string RecipientUserId,
    string RecipientName,
    bool IsRead,
    DateTime? ReadAt,
    string DeliveryStatus,
    int DeliveryAttempts,
    DateTime? LastAttemptedAt,
    DateTime? DeliveredAt,
    string? LastDeliveryError,
    string? ProviderReference);

public record CreateNotificationRequest(
    string Title,
    string Body,
    MessageChannel Channel,
    IReadOnlyList<string> RecipientUserIds,
    DateTime? ScheduledAt);

public record CancelNotificationRequest(string Reason);

public record UpdateScheduledNotificationRequest(string Title, string Body, MessageChannel Channel, DateTime? ScheduledAt);

public record UpdateNotificationDeliveryRequest(
    ChannelDeliveryStatus Status,
    string? ProviderReference,
    string? Error);

public record ChannelDeliveryLogResponse(
    int Id,
    string Channel,
    string Recipient,
    string Subject,
    string Status,
    string? ProviderReference,
    string? Error,
    DateTime? SentAt,
    DateTime CreatedAt);
