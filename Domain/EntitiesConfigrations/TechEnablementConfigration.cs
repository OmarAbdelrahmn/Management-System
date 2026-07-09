using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class TechSystemSettingConfigration : IEntityTypeConfiguration<TechSystemSetting>
{
    public void Configure(EntityTypeBuilder<TechSystemSetting> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Category).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Value).HasMaxLength(2000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Key).IsUnique();
        entity.HasIndex(x => new { x.Category, x.Status });
    }
}

public class OrganizationAssignmentConfigration : IEntityTypeConfiguration<OrganizationAssignment>
{
    public void Configure(EntityTypeBuilder<OrganizationAssignment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UnitName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.AssigneeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.RoleTitle).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.AssignmentType, x.IsActive });
    }
}

public class VisualAssetTemplateConfigration : IEntityTypeConfiguration<VisualAssetTemplate>
{
    public void Configure(EntityTypeBuilder<VisualAssetTemplate> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.FileUrl).HasMaxLength(500);
        entity.Property(x => x.DesignJson).HasMaxLength(4000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.AssetType, x.IsActive });
    }
}

public class CybersecurityReviewConfigration : IEntityTypeConfiguration<CybersecurityReview>
{
    public void Configure(EntityTypeBuilder<CybersecurityReview> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Area).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Finding).IsRequired().HasMaxLength(1000);
        entity.Property(x => x.Severity).IsRequired().HasMaxLength(40);
        entity.Property(x => x.Owner).HasMaxLength(200);
        entity.Property(x => x.MitigationPlan).HasMaxLength(1500);
        entity.HasIndex(x => new { x.Status, x.Severity });
    }
}

public class NcnpDataRecordConfigration : IEntityTypeConfiguration<NcnpDataRecord>
{
    public void Configure(EntityTypeBuilder<NcnpDataRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(100);
        entity.Property(x => x.BeneficiaryName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.SupportType).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Cost).HasPrecision(18, 2);
        entity.Property(x => x.PlatformReference).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ReferenceNumber).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}
