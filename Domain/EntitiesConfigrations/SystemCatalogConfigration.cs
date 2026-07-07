using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class SystemCatalogConfigration :
    IEntityTypeConfiguration<SystemModule>,
    IEntityTypeConfiguration<SystemPage>
{
    public void Configure(EntityTypeBuilder<SystemModule> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(80);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(250);
        entity.Property(x => x.NameEn).HasMaxLength(250);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.HasIndex(x => x.Key).IsUnique();
    }

    public void Configure(EntityTypeBuilder<SystemPage> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Route).IsRequired().HasMaxLength(250);
        entity.Property(x => x.PermissionKey).IsRequired().HasMaxLength(180);
        entity.Property(x => x.ServiceName).IsRequired().HasMaxLength(180);
        entity.Property(x => x.ServicePlan).HasMaxLength(4000);
        entity.Property(x => x.UiPlan).HasMaxLength(4000);
        entity.HasIndex(x => x.Key).IsUnique();
        entity.HasOne(x => x.SystemModule).WithMany(x => x.Pages).HasForeignKey(x => x.SystemModuleId).OnDelete(DeleteBehavior.Restrict);
    }
}
