using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class ArchiveDocumentConfigration : IEntityTypeConfiguration<ArchiveDocument>
{
    public void Configure(EntityTypeBuilder<ArchiveDocument> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DocumentNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.FileUrl).HasMaxLength(500);
        entity.Property(x => x.OwnerDepartment).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.DocumentNumber).IsUnique();
        entity.HasIndex(x => new { x.Category, x.Status });
    }
}

public class CorrespondenceRecordConfigration : IEntityTypeConfiguration<CorrespondenceRecord>
{
    public void Configure(EntityTypeBuilder<CorrespondenceRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MailNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.PartyName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.BarcodeValue).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.MailNumber).IsUnique();
        entity.HasIndex(x => new { x.Direction, x.Status });
    }
}

public class CorrespondenceOperationConfigration : IEntityTypeConfiguration<CorrespondenceOperation>
{
    public void Configure(EntityTypeBuilder<CorrespondenceOperation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.OperationNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.AssignedTo).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.OperationNumber).IsUnique();
        entity.HasIndex(x => new { x.Status, x.DueDate });
        entity.HasOne(x => x.CorrespondenceRecord).WithMany(x => x.Operations).HasForeignKey(x => x.CorrespondenceRecordId).OnDelete(DeleteBehavior.Cascade);
    }
}
