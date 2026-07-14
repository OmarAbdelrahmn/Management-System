using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Emails;
using Application.Contracts.Messaging;
using Application.Service.Emails;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Messaging;

public class MessagingService(
    ApplicationDbcontext dbcontext,
    ICurrentUserContext currentUserContext,
    IEmailService emailService) : IMessagingService
{
    public async Task<Result<IEnumerable<InternalMailResponse>>> GetInboxAsync(string userId, CancellationToken cancellationToken = default)
    {
        var messages = await dbcontext.InternalMailMessages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser)
            .Where(x => x.Status == InternalMailStatus.Sent && x.Recipients.Any(r => r.RecipientUserId == userId && !r.IsArchived))
            .OrderByDescending(x => x.SentAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<InternalMailResponse>>(messages.Select(MapMail));
    }

    public async Task<Result<IEnumerable<InternalMailResponse>>> GetSentAsync(string userId, CancellationToken cancellationToken = default)
    {
        var messages = await dbcontext.InternalMailMessages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser)
            .Where(x => x.SenderUserId == userId && x.Status == InternalMailStatus.Sent)
            .OrderByDescending(x => x.SentAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<InternalMailResponse>>(messages.Select(MapMail));
    }

    public async Task<Result<IEnumerable<InternalMailResponse>>> GetArchivedAsync(string userId, CancellationToken cancellationToken = default)
    {
        var messages = await dbcontext.InternalMailMessages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser)
            .Where(x => x.Status == InternalMailStatus.Sent && x.Recipients.Any(r => r.RecipientUserId == userId && r.IsArchived))
            .OrderByDescending(x => x.SentAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<InternalMailResponse>>(messages.Select(MapMail));
    }

    public async Task<Result<IEnumerable<InternalMailResponse>>> GetDraftsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var messages = await dbcontext.InternalMailMessages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser)
            .Where(x => x.SenderUserId == userId && x.Status == InternalMailStatus.Draft)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<InternalMailResponse>>(messages.Select(MapMail));
    }

    public async Task<Result<InternalMailResponse>> CreateMailAsync(CreateInternalMailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<InternalMailResponse>(MessagingErrors.InvalidRequest);

        var senderUserId = currentUserContext.UserId;
        if (string.IsNullOrWhiteSpace(senderUserId) || !await dbcontext.Users.AnyAsync(x => x.Id == senderUserId, cancellationToken))
            return Result.Failure<InternalMailResponse>(MessagingErrors.InvalidRequest);

        var recipientIds = request.RecipientUserIds.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (request.SendNow && recipientIds.Count == 0)
            return Result.Failure<InternalMailResponse>(MessagingErrors.InvalidRequest);

        if (!await AllUsersExistAsync(recipientIds, cancellationToken))
            return Result.Failure<InternalMailResponse>(MessagingErrors.UserNotFound);

        var message = new InternalMailMessage
        {
            SenderUserId = senderUserId,
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            Status = request.SendNow ? InternalMailStatus.Sent : InternalMailStatus.Draft,
            SentAt = request.SendNow ? DateTime.UtcNow.AddHours(3) : null,
            Recipients = recipientIds.Select(x => new InternalMailRecipient { RecipientUserId = x }).ToList()
        };

        dbcontext.InternalMailMessages.Add(message);
        await dbcontext.SaveChangesAsync(cancellationToken);

        var created = await MailQuery().FirstAsync(x => x.Id == message.Id, cancellationToken);
        return Result.Success(MapMail(created));
    }

    public async Task<Result<InternalMailResponse>> UpdateDraftAsync(int id, UpdateInternalMailDraftRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<InternalMailResponse>(MessagingErrors.InvalidRequest);

        var message = await dbcontext.InternalMailMessages
            .Include(x => x.Recipients)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (message is null)
            return Result.Failure<InternalMailResponse>(MessagingErrors.NotFound);

        if (message.Status != InternalMailStatus.Draft || message.SenderUserId != currentUserContext.UserId)
            return Result.Failure<InternalMailResponse>(MessagingErrors.InvalidRequest);

        var recipientIds = request.RecipientUserIds.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (!await AllUsersExistAsync(recipientIds, cancellationToken))
            return Result.Failure<InternalMailResponse>(MessagingErrors.UserNotFound);

        message.Subject = request.Subject.Trim();
        message.Body = request.Body.Trim();
        message.Recipients.Clear();
        foreach (var recipientId in recipientIds)
            message.Recipients.Add(new InternalMailRecipient { RecipientUserId = recipientId });

        await dbcontext.SaveChangesAsync(cancellationToken);
        var updated = await MailQuery().FirstAsync(x => x.Id == message.Id, cancellationToken);
        return Result.Success(MapMail(updated));
    }

    public async Task<Result> SendDraftAsync(int id, SendDraftRequest request, CancellationToken cancellationToken = default)
    {
        var message = await dbcontext.InternalMailMessages.Include(x => x.Recipients).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (message is null)
            return Result.Failure(MessagingErrors.NotFound);

        if (message.Status != InternalMailStatus.Draft || message.SenderUserId != currentUserContext.UserId)
            return Result.Failure(MessagingErrors.InvalidRequest);

        var recipientIds = request.RecipientUserIds.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (recipientIds.Count == 0 || !await AllUsersExistAsync(recipientIds, cancellationToken))
            return Result.Failure(MessagingErrors.UserNotFound);

        message.Recipients.Clear();
        foreach (var recipientId in recipientIds)
            message.Recipients.Add(new InternalMailRecipient { RecipientUserId = recipientId });

        message.Status = InternalMailStatus.Sent;
        message.SentAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CancelDraftAsync(int id, CancellationToken cancellationToken = default)
    {
        var message = await dbcontext.InternalMailMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (message is null)
            return Result.Failure(MessagingErrors.NotFound);

        if (message.Status != InternalMailStatus.Draft || message.SenderUserId != currentUserContext.UserId)
            return Result.Failure(MessagingErrors.InvalidRequest);

        message.Status = InternalMailStatus.Cancelled;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MarkMailReadAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var recipient = await dbcontext.InternalMailRecipients.FirstOrDefaultAsync(x => x.Id == recipientId, cancellationToken);
        if (recipient is null)
            return Result.Failure(MessagingErrors.NotFound);

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow.AddHours(3);
            await dbcontext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ArchiveMailRecipientAsync(int recipientId, bool isArchived, CancellationToken cancellationToken = default)
    {
        var recipient = await dbcontext.InternalMailRecipients.FirstOrDefaultAsync(x => x.Id == recipientId, cancellationToken);
        if (recipient is null)
            return Result.Failure(MessagingErrors.NotFound);

        if (recipient.RecipientUserId != currentUserContext.UserId && !currentUserContext.Roles.Contains("Admin"))
            return Result.Failure(MessagingErrors.InvalidRequest);

        recipient.IsArchived = isArchived;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<MessageTemplateResponse>>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await dbcontext.MessageTemplates
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.NameAr)
            .Select(x => MapTemplate(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MessageTemplateResponse>>(templates);
    }

    public async Task<Result<MessageTemplateResponse>> SaveTemplateAsync(int? id, UpsertMessageTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.Subject))
            return Result.Failure<MessageTemplateResponse>(MessagingErrors.InvalidRequest);

        MessageTemplate template;
        if (id.HasValue)
        {
            var existingTemplate = await dbcontext.MessageTemplates.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (existingTemplate is null)
                return Result.Failure<MessageTemplateResponse>(MessagingErrors.TemplateNotFound);

            template = existingTemplate;
        }
        else
        {
            template = new MessageTemplate();
            dbcontext.MessageTemplates.Add(template);
        }

        template.Key = request.Key.Trim();
        template.NameAr = request.NameAr.Trim();
        template.Subject = request.Subject.Trim();
        template.Body = request.Body.Trim();
        template.Channel = request.Channel;
        template.IsActive = request.IsActive;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapTemplate(template));
    }

    public async Task<Result<IEnumerable<NotificationResponse>>> GetNotificationsAsync(NotificationStatus? status = null, MessageChannel? channel = null, string? keyword = null, CancellationToken cancellationToken = default)
    {
        var query = NotificationQuery();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (channel.HasValue)
            query = query.Where(x => x.Channel == channel.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(x =>
                x.Title.Contains(normalizedKeyword) ||
                x.Body.Contains(normalizedKeyword) ||
                x.CreatedBySystemUser!.FullName.Contains(normalizedKeyword));
        }

        var notifications = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<NotificationResponse>>(notifications.Select(MapNotification));
    }

    public async Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<NotificationResponse>(MessagingErrors.InvalidRequest);

        var creatorUserId = currentUserContext.UserId;
        if (string.IsNullOrWhiteSpace(creatorUserId))
            return Result.Failure<NotificationResponse>(MessagingErrors.InvalidRequest);

        var recipientIds = request.RecipientUserIds.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (recipientIds.Count == 0 || !await AllUsersExistAsync(recipientIds, cancellationToken))
            return Result.Failure<NotificationResponse>(MessagingErrors.UserNotFound);

        var now = DateTime.UtcNow.AddHours(3);
        var isScheduled = request.ScheduledAt.HasValue && request.ScheduledAt.Value > now;
        var initialDeliveryStatus = request.Channel == MessageChannel.Internal && !isScheduled
            ? ChannelDeliveryStatus.Sent
            : ChannelDeliveryStatus.Pending;

        var notification = new SystemNotification
        {
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Channel = request.Channel,
            CreatedBySystemUserId = creatorUserId,
            ScheduledAt = request.ScheduledAt,
            Recipients = recipientIds.Select(x => new SystemNotificationRecipient
            {
                RecipientUserId = x,
                DeliveryStatus = initialDeliveryStatus,
                DeliveryAttempts = isScheduled ? 0 : 1,
                LastAttemptedAt = isScheduled ? null : now,
                DeliveredAt = initialDeliveryStatus == ChannelDeliveryStatus.Sent ? now : null
            }).ToList()
        };

        dbcontext.SystemNotifications.Add(notification);
        if (!isScheduled)
            await AddChannelOutputsAsync(request.Channel, request.Title.Trim(), request.Body.Trim(), recipientIds, cancellationToken);

        await dbcontext.SaveChangesAsync(cancellationToken);

        var created = await NotificationQuery().FirstAsync(x => x.Id == notification.Id, cancellationToken);
        return Result.Success(MapNotification(created));
    }

    public async Task<Result> CancelNotificationAsync(int id, CancelNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = await dbcontext.SystemNotifications
            .Include(x => x.Recipients)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notification is null)
            return Result.Failure(MessagingErrors.NotificationNotFound);

        if (notification.Status != NotificationStatus.Active || string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(MessagingErrors.InvalidRequest);

        var reason = request.Reason.Trim();
        notification.Status = NotificationStatus.Cancelled;
        notification.CancelledAt = DateTime.UtcNow.AddHours(3);
        notification.CancellationReason = reason;
        foreach (var recipient in notification.Recipients.Where(x => x.DeliveryStatus == ChannelDeliveryStatus.Pending))
        {
            recipient.DeliveryStatus = ChannelDeliveryStatus.Cancelled;
            recipient.LastDeliveryError = reason;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<NotificationResponse>> UpdateScheduledNotificationAsync(int id, UpdateScheduledNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            return Result.Failure<NotificationResponse>(MessagingErrors.InvalidRequest);

        var notification = await dbcontext.SystemNotifications.Include(x => x.Recipients).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notification is null)
            return Result.Failure<NotificationResponse>(MessagingErrors.NotificationNotFound);

        var now = DateTime.UtcNow.AddHours(3);
        if (notification.Status != NotificationStatus.Active || !notification.ScheduledAt.HasValue || notification.ScheduledAt <= now || !request.ScheduledAt.HasValue || request.ScheduledAt <= now || notification.Recipients.Any(x => x.DeliveryStatus != ChannelDeliveryStatus.Pending))
            return Result.Failure<NotificationResponse>(MessagingErrors.InvalidRequest);

        notification.Title = request.Title.Trim();
        notification.Body = request.Body.Trim();
        notification.Channel = request.Channel;
        notification.ScheduledAt = request.ScheduledAt;
        await dbcontext.SaveChangesAsync(cancellationToken);
        var updated = await NotificationQuery().FirstAsync(x => x.Id == id, cancellationToken);
        return Result.Success(MapNotification(updated));
    }

    public async Task<Result<NotificationRecipientResponse>> MarkNotificationRecipientReadAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var recipient = await NotificationRecipientQuery()
            .FirstOrDefaultAsync(x => x.Id == recipientId, cancellationToken);

        if (recipient is null)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.RecipientNotFound);

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow.AddHours(3);
            await dbcontext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(MapNotificationRecipient(recipient));
    }

    public async Task<Result<NotificationRecipientResponse>> RecordNotificationDeliveryAsync(int recipientId, UpdateNotificationDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        var recipient = await NotificationRecipientQuery()
            .FirstOrDefaultAsync(x => x.Id == recipientId, cancellationToken);

        if (recipient is null)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.RecipientNotFound);

        var notification = recipient.SystemNotification;
        if (notification is null)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.NotificationNotFound);

        var now = DateTime.UtcNow.AddHours(3);
        recipient.DeliveryStatus = request.Status;
        recipient.ProviderReference = string.IsNullOrWhiteSpace(request.ProviderReference) ? null : request.ProviderReference.Trim();
        recipient.LastDeliveryError = string.IsNullOrWhiteSpace(request.Error) ? null : request.Error.Trim();
        recipient.LastAttemptedAt = now;
        recipient.DeliveryAttempts = Math.Max(1, recipient.DeliveryAttempts);
        recipient.DeliveredAt = request.Status == ChannelDeliveryStatus.Sent ? now : null;

        dbcontext.ChannelDeliveryLogs.Add(new ChannelDeliveryLog
        {
            Channel = notification.Channel,
            Recipient = ResolveRecipientAddress(notification.Channel, recipient.RecipientUser?.FullName, recipient.RecipientUser?.Email, recipient.RecipientUser?.PhoneNumber),
            Subject = notification.Title,
            Body = notification.Body,
            Status = request.Status,
            ProviderReference = recipient.ProviderReference,
            Error = recipient.LastDeliveryError,
            SentAt = request.Status == ChannelDeliveryStatus.Sent ? now : null
        });

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapNotificationRecipient(recipient));
    }

    public async Task<Result<NotificationRecipientResponse>> RetryNotificationRecipientAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        var recipient = await NotificationRecipientQuery()
            .FirstOrDefaultAsync(x => x.Id == recipientId, cancellationToken);

        if (recipient is null)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.RecipientNotFound);

        var notification = recipient.SystemNotification;
        if (notification is null)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.NotificationNotFound);

        if (notification.Status == NotificationStatus.Cancelled)
            return Result.Failure<NotificationRecipientResponse>(MessagingErrors.NotificationCancelled);

        var now = DateTime.UtcNow.AddHours(3);
        recipient.DeliveryAttempts += 1;
        recipient.LastAttemptedAt = now;
        recipient.LastDeliveryError = null;
        recipient.ProviderReference = null;

        if (notification.Channel == MessageChannel.Internal)
        {
            recipient.DeliveryStatus = ChannelDeliveryStatus.Sent;
            recipient.DeliveredAt = now;
        }
        else
        {
            recipient.DeliveryStatus = ChannelDeliveryStatus.Pending;
            recipient.DeliveredAt = null;
        }

        QueueRecipientOutput(notification, recipient, now);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapNotificationRecipient(recipient));
    }

    public async Task<Result<IEnumerable<ChannelDeliveryLogResponse>>> GetChannelLogsAsync(MessageChannel? channel = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ChannelDeliveryLogs.AsNoTracking();
        if (channel.HasValue)
            query = query.Where(x => x.Channel == channel.Value);

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(300)
            .Select(x => MapChannelLog(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ChannelDeliveryLogResponse>>(logs);
    }

    public Task<Result<IEnumerable<EmailOutboxResponse>>> GetEmailOutboxAsync(bool? sent = null, CancellationToken cancellationToken = default) =>
        emailService.GetOutboxAsync(sent, cancellationToken);

    private IQueryable<InternalMailMessage> MailQuery() =>
        dbcontext.InternalMailMessages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser);

    private IQueryable<SystemNotification> NotificationQuery() =>
        dbcontext.SystemNotifications
            .AsNoTracking()
            .Include(x => x.CreatedBySystemUser)
            .Include(x => x.Recipients)
            .ThenInclude(x => x.RecipientUser);

    private IQueryable<SystemNotificationRecipient> NotificationRecipientQuery() =>
        dbcontext.SystemNotificationRecipients
            .Include(x => x.SystemNotification)
            .Include(x => x.RecipientUser);

    private async Task<bool> AllUsersExistAsync(IReadOnlyCollection<string> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
            return true;

        var count = await dbcontext.Users.CountAsync(x => userIds.Contains(x.Id), cancellationToken);
        return count == userIds.Count;
    }

    private async Task AddChannelOutputsAsync(MessageChannel channel, string subject, string body, IReadOnlyCollection<string> recipientUserIds, CancellationToken cancellationToken)
    {
        var users = await dbcontext.Users
            .Where(x => recipientUserIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Email, x.PhoneNumber, x.FullName })
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            if (channel == MessageChannel.Email && !string.IsNullOrWhiteSpace(user.Email))
                dbcontext.EmailOutbox.Add(new EmailOutbox { ToEmail = user.Email, Subject = subject, Body = body });

            var recipient = ResolveRecipientAddress(channel, user.FullName, user.Email, user.PhoneNumber);
            var status = channel == MessageChannel.Internal ? ChannelDeliveryStatus.Sent : ChannelDeliveryStatus.Pending;

            dbcontext.ChannelDeliveryLogs.Add(new ChannelDeliveryLog
            {
                Channel = channel,
                Recipient = recipient,
                Subject = subject,
                Body = body,
                Status = status,
                SentAt = status == ChannelDeliveryStatus.Sent ? DateTime.UtcNow.AddHours(3) : null
            });
        }
    }

    private void QueueRecipientOutput(SystemNotification notification, SystemNotificationRecipient recipient, DateTime now)
    {
        if (notification.Channel == MessageChannel.Email && !string.IsNullOrWhiteSpace(recipient.RecipientUser?.Email))
            dbcontext.EmailOutbox.Add(new EmailOutbox { ToEmail = recipient.RecipientUser.Email, Subject = notification.Title, Body = notification.Body });

        dbcontext.ChannelDeliveryLogs.Add(new ChannelDeliveryLog
        {
            Channel = notification.Channel,
            Recipient = ResolveRecipientAddress(notification.Channel, recipient.RecipientUser?.FullName, recipient.RecipientUser?.Email, recipient.RecipientUser?.PhoneNumber),
            Subject = notification.Title,
            Body = notification.Body,
            Status = recipient.DeliveryStatus,
            SentAt = recipient.DeliveryStatus == ChannelDeliveryStatus.Sent ? now : null
        });
    }

    private static string ResolveRecipientAddress(MessageChannel channel, string? fullName, string? email, string? phoneNumber) =>
        channel switch
        {
            MessageChannel.Email => email ?? fullName ?? string.Empty,
            MessageChannel.Sms => phoneNumber ?? fullName ?? string.Empty,
            _ => fullName ?? email ?? phoneNumber ?? string.Empty
        };

    private static InternalMailResponse MapMail(InternalMailMessage message) =>
        new(
            message.Id,
            message.SenderUserId,
            message.SenderUser?.FullName ?? string.Empty,
            message.Subject,
            message.Body,
            message.Status.ToString(),
            message.SentAt,
            message.CreatedAt,
            message.Recipients.Select(x => new InternalMailRecipientResponse(x.Id, x.RecipientUserId, x.RecipientUser?.FullName ?? string.Empty, x.IsRead, x.ReadAt, x.IsArchived)).ToList());

    private static MessageTemplateResponse MapTemplate(MessageTemplate template) =>
        new(template.Id, template.Key, template.NameAr, template.Subject, template.Body, template.Channel.ToString(), template.IsActive);

    private static NotificationResponse MapNotification(SystemNotification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.Channel.ToString(),
            notification.Status.ToString(),
            notification.CreatedBySystemUserId,
            notification.CreatedBySystemUser?.FullName ?? string.Empty,
            notification.ScheduledAt,
            notification.CancelledAt,
            notification.CancellationReason,
            notification.CreatedAt,
            notification.Recipients.Select(MapNotificationRecipient).ToList());

    private static NotificationRecipientResponse MapNotificationRecipient(SystemNotificationRecipient recipient) =>
        new(
            recipient.Id,
            recipient.RecipientUserId,
            recipient.RecipientUser?.FullName ?? string.Empty,
            recipient.IsRead,
            recipient.ReadAt,
            recipient.DeliveryStatus.ToString(),
            recipient.DeliveryAttempts,
            recipient.LastAttemptedAt,
            recipient.DeliveredAt,
            recipient.LastDeliveryError,
            recipient.ProviderReference);

    private static ChannelDeliveryLogResponse MapChannelLog(ChannelDeliveryLog log) =>
        new(log.Id, log.Channel.ToString(), log.Recipient, log.Subject, log.Status.ToString(), log.ProviderReference, log.Error, log.SentAt, log.CreatedAt);
}
