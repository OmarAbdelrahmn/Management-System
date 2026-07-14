using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class SystemReportDefinitionConfigration : IEntityTypeConfiguration<SystemReportDefinition>
{
    public void Configure(EntityTypeBuilder<SystemReportDefinition> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Key).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(250);
        entity.Property(x => x.SourceDomain).IsRequired().HasMaxLength(120);
        entity.HasIndex(x => x.Key).IsUnique();
        entity.HasIndex(x => new { x.Kind, x.IsActive });
    }
}

public class SystemReportRunConfigration : IEntityTypeConfiguration<SystemReportRun>
{
    public void Configure(EntityTypeBuilder<SystemReportRun> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReportKey).IsRequired().HasMaxLength(120);
        entity.Property(x => x.ReportName).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Format).IsRequired().HasMaxLength(60);
        entity.Property(x => x.FiltersJson).HasMaxLength(4000);
        entity.Property(x => x.RequestedBy).HasMaxLength(200);
        entity.Property(x => x.ArchiveFileName).HasMaxLength(300);
        entity.Property(x => x.ArchiveContentType).HasMaxLength(160);
        entity.HasIndex(x => new { x.ReportKey, x.GeneratedAt });
        entity.HasOne(x => x.SystemReportDefinition).WithMany(x => x.Runs).HasForeignKey(x => x.SystemReportDefinitionId).OnDelete(DeleteBehavior.SetNull);
    }
}
