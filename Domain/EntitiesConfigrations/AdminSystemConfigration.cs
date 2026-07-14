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
        entity.HasIndex(x => x.DeletedAt);

        entity.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FileAssetVersionConfigration : IEntityTypeConfiguration<FileAssetVersion>
{
    public void Configure(EntityTypeBuilder<FileAssetVersion> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        entity.Property(x => x.ContentType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.StoragePath).IsRequired().HasMaxLength(600);
        entity.Property(x => x.Sha256).IsRequired().HasMaxLength(64);
        entity.Property(x => x.UploadedByUserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.FileAssetId, x.VersionNumber }).IsUnique();
        entity.HasOne(x => x.FileAsset).WithMany(x => x.Versions).HasForeignKey(x => x.FileAssetId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FileAssetLinkConfigration : IEntityTypeConfiguration<FileAssetLink>
{
    public void Configure(EntityTypeBuilder<FileAssetLink> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EntityType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.EntityId).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Label).HasMaxLength(260);
        entity.HasIndex(x => new { x.EntityType, x.EntityId });
        entity.HasIndex(x => new { x.FileAssetId, x.EntityType, x.EntityId }).IsUnique();
        entity.HasOne(x => x.FileAsset).WithMany(x => x.Links).HasForeignKey(x => x.FileAssetId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SavedQueryViewConfigration : IEntityTypeConfiguration<SavedQueryView>
{
    public void Configure(EntityTypeBuilder<SavedQueryView> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.ScreenKey).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
        entity.Property(x => x.FilterJson).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => new { x.UserId, x.ScreenKey, x.Name }).IsUnique();
    }
}
