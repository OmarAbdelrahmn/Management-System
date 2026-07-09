using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class InternalMailMessageConfigration : IEntityTypeConfiguration<InternalMailMessage>
{
    public void Configure(EntityTypeBuilder<InternalMailMessage> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SenderUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.SentAt);

        entity.HasOne(x => x.SenderUser)
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InternalMailRecipientConfigration : IEntityTypeConfiguration<InternalMailRecipient>
{
    public void Configure(EntityTypeBuilder<InternalMailRecipient> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RecipientUserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.RecipientUserId, x.IsRead });

        entity.HasOne(x => x.InternalMailMessage)
            .WithMany(x => x.Recipients)
            .HasForeignKey(x => x.InternalMailMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.RecipientUser)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MessageTemplateConfigration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(100);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => x.Key).IsUnique();
    }
}

public class SystemNotificationConfigration : IEntityTypeConfiguration<SystemNotification>
{
    public void Configure(EntityTypeBuilder<SystemNotification> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.CreatedBySystemUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.CancellationReason).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.ScheduledAt);

        entity.HasOne(x => x.CreatedBySystemUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBySystemUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SystemNotificationRecipientConfigration : IEntityTypeConfiguration<SystemNotificationRecipient>
{
    public void Configure(EntityTypeBuilder<SystemNotificationRecipient> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RecipientUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.LastDeliveryError).HasMaxLength(2000);
        entity.Property(x => x.ProviderReference).HasMaxLength(160);
        entity.HasIndex(x => new { x.RecipientUserId, x.IsRead });
        entity.HasIndex(x => new { x.RecipientUserId, x.DeliveryStatus });
        entity.HasIndex(x => x.DeliveryStatus);

        entity.HasOne(x => x.SystemNotification)
            .WithMany(x => x.Recipients)
            .HasForeignKey(x => x.SystemNotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.RecipientUser)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChannelDeliveryLogConfigration : IEntityTypeConfiguration<ChannelDeliveryLog>
{
    public void Configure(EntityTypeBuilder<ChannelDeliveryLog> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Recipient).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.ProviderReference).HasMaxLength(160);
        entity.Property(x => x.Error).HasMaxLength(2000);
        entity.HasIndex(x => x.Channel);
        entity.HasIndex(x => x.Status);
    }
}
