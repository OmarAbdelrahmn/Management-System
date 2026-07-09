using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class EstablishmentDocumentConfigration : IEntityTypeConfiguration<EstablishmentDocument>
{
    public void Configure(EntityTypeBuilder<EstablishmentDocument> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DocumentCode).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.OwnerDepartment).HasMaxLength(160);
        entity.Property(x => x.FileUrl).HasMaxLength(500);
        entity.Property(x => x.HelperNotes).HasMaxLength(1500);
        entity.HasIndex(x => x.DocumentCode).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class AidCommitteeCreditEntryConfigration : IEntityTypeConfiguration<AidCommitteeCreditEntry>
{
    public void Configure(EntityTypeBuilder<AidCommitteeCreditEntry> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EntryNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reference).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.EntryNumber).IsUnique();
    }
}

public class ExecutiveApprovalRequestConfigration : IEntityTypeConfiguration<ExecutiveApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ExecutiveApprovalRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(200);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.ApprovalKind, x.Status });
    }
}

public class PaymentAuthorizationConfigration : IEntityTypeConfiguration<PaymentAuthorization>
{
    public void Configure(EntityTypeBuilder<PaymentAuthorization> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AuthorizationNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.PayeeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Purpose).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RejectionNote).HasMaxLength(1000);
        entity.HasIndex(x => x.AuthorizationNumber).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class AdministrativeDecisionRecordConfigration : IEntityTypeConfiguration<AdministrativeDecisionRecord>
{
    public void Configure(EntityTypeBuilder<AdministrativeDecisionRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DecisionNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.RelatedMeetingCode).HasMaxLength(120);
        entity.Property(x => x.AssignedTo).HasMaxLength(200);
        entity.Property(x => x.ExportTemplateName).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.DecisionNumber).IsUnique();
        entity.HasIndex(x => new { x.DecisionType, x.Status });
    }
}
