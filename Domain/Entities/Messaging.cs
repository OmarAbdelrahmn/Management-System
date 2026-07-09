using Domain.Auditing;
using Domain.Identity;

namespace Domain.Entities;

public enum InternalMailStatus
{
    Draft = 0,
    Sent = 1,
    Archived = 2,
    Cancelled = 3
}

public enum NotificationStatus
{
    Active = 0,
    Cancelled = 1,
    Expired = 2
}

public enum MessageChannel
{
    Internal = 0,
    Email = 1,
    Sms = 2,
    Push = 3
}

public enum ChannelDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public class InternalMailMessage : IAuditable
{
    public int Id { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public InternalMailStatus Status { get; set; } = InternalMailStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationUser? SenderUser { get; set; }
    public ICollection<InternalMailRecipient> Recipients { get; set; } = new List<InternalMailRecipient>();
}

public class InternalMailRecipient : IAuditable
{
    public int Id { get; set; }
    public int InternalMailMessageId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public InternalMailMessage? InternalMailMessage { get; set; }
    public ApplicationUser? RecipientUser { get; set; }
}

public class MessageTemplate : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageChannel Channel { get; set; } = MessageChannel.Internal;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class SystemNotification : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageChannel Channel { get; set; } = MessageChannel.Internal;
    public NotificationStatus Status { get; set; } = NotificationStatus.Active;
    public string CreatedBySystemUserId { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationUser? CreatedBySystemUser { get; set; }
    public ICollection<SystemNotificationRecipient> Recipients { get; set; } = new List<SystemNotificationRecipient>();
}

public class SystemNotificationRecipient : IAuditable
{
    public int Id { get; set; }
    public int SystemNotificationId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public ChannelDeliveryStatus DeliveryStatus { get; set; } = ChannelDeliveryStatus.Pending;
    public int DeliveryAttempts { get; set; }
    public DateTime? LastAttemptedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? LastDeliveryError { get; set; }
    public string? ProviderReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public SystemNotification? SystemNotification { get; set; }
    public ApplicationUser? RecipientUser { get; set; }
}

public class ChannelDeliveryLog : IAuditable
{
    public int Id { get; set; }
    public MessageChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ChannelDeliveryStatus Status { get; set; } = ChannelDeliveryStatus.Pending;
    public string? ProviderReference { get; set; }
    public string? Error { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
