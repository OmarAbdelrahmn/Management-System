using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class SystemLogConfigration :
    IEntityTypeConfiguration<AuditLog>,
    IEntityTypeConfiguration<EmailOutbox>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ActorUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
        entity.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
        entity.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Details).HasMaxLength(2000);
    }

    public void Configure(EntityTypeBuilder<EmailOutbox> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ToEmail).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.Error).HasMaxLength(2000);
    }
}
