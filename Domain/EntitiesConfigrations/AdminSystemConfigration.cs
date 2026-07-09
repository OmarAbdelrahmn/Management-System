using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class AppPermissionConfigration : IEntityTypeConfiguration<AppPermission>
{
    public void Configure(EntityTypeBuilder<AppPermission> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(160);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Category).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.HasIndex(x => x.Key).IsUnique();
    }
}

public class RolePermissionConfigration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RoleId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.RoleId, x.AppPermissionId }).IsUnique();

        entity.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.AppPermission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.AppPermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserLoginAuditConfigration : IEntityTypeConfiguration<UserLoginAudit>
{
    public void Configure(EntityTypeBuilder<UserLoginAudit> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).HasMaxLength(450);
        entity.Property(x => x.UserName).IsRequired().HasMaxLength(256);
        entity.Property(x => x.FailureReason).HasMaxLength(1000);
        entity.Property(x => x.IpAddress).HasMaxLength(80);
        entity.Property(x => x.UserAgent).HasMaxLength(600);
        entity.HasIndex(x => x.AttemptedAt);
        entity.HasIndex(x => x.Result);

        entity.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SystemSettingConfigration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(160);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Value).HasMaxLength(4000);
        entity.Property(x => x.Category).IsRequired().HasMaxLength(100);
        entity.HasIndex(x => x.Key).IsUnique();
    }
}

public class FileAssetConfigration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        entity.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        entity.Property(x => x.ContentType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.StoragePath).IsRequired().HasMaxLength(600);
        entity.Property(x => x.Category).IsRequired().HasMaxLength(100);
        entity.Property(x => x.UploadedByUserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => x.Category);

        entity.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
