using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class BeneficiaryProfileConfigration : IEntityTypeConfiguration<BeneficiaryProfile>
{
    public void Configure(EntityTypeBuilder<BeneficiaryProfile> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BeneficiaryNumber).IsRequired().HasMaxLength(40);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(30);
        entity.Property(x => x.Gender).HasMaxLength(30);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.City).HasMaxLength(120);
        entity.Property(x => x.Address).HasMaxLength(500);
        entity.Property(x => x.Category).HasMaxLength(80);
        entity.Property(x => x.Grade).HasMaxLength(80);
        entity.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.ArchiveReason).HasMaxLength(1000);
        entity.HasIndex(x => x.BeneficiaryNumber).IsUnique();
        entity.HasIndex(x => x.NationalId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.City);
        entity.HasIndex(x => x.Category);
    }
}

public class BeneficiaryDependentConfigration : IEntityTypeConfiguration<BeneficiaryDependent>
{
    public void Configure(EntityTypeBuilder<BeneficiaryDependent> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(30);
        entity.Property(x => x.Relationship).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Category).HasMaxLength(80);
        entity.Property(x => x.Grade).HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.BeneficiaryProfileId, x.IsActive });

        entity.HasOne(x => x.BeneficiaryProfile)
            .WithMany(x => x.Dependents)
            .HasForeignKey(x => x.BeneficiaryProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BeneficiaryGuardianConfigration : IEntityTypeConfiguration<BeneficiaryGuardian>
{
    public void Configure(EntityTypeBuilder<BeneficiaryGuardian> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(30);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Relationship).IsRequired().HasMaxLength(80);
        entity.Property(x => x.DeleteReason).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.BeneficiaryProfileId, x.IsPrimary });
        entity.HasIndex(x => new { x.BeneficiaryProfileId, x.IsDeleted });

        entity.HasOne(x => x.BeneficiaryProfile)
            .WithMany(x => x.Guardians)
            .HasForeignKey(x => x.BeneficiaryProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BeneficiaryAccountArtifactConfigration : IEntityTypeConfiguration<BeneficiaryAccountArtifact>
{
    public void Configure(EntityTypeBuilder<BeneficiaryAccountArtifact> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.HolderName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Source).HasMaxLength(120);
        entity.Property(x => x.Payload).HasMaxLength(2000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ReferenceNumber).IsUnique();
        entity.HasIndex(x => new { x.Type, x.Status });
        entity.HasIndex(x => x.BeneficiaryProfileId);
        entity.HasIndex(x => x.BeneficiaryDependentId);

        entity.HasOne(x => x.BeneficiaryProfile)
            .WithMany(x => x.AccountArtifacts)
            .HasForeignKey(x => x.BeneficiaryProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.BeneficiaryDependent)
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryDependentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BeneficiaryGuardianOperationConfigration : IEntityTypeConfiguration<BeneficiaryGuardianOperation>
{
    public void Configure(EntityTypeBuilder<BeneficiaryGuardianOperation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.SubjectName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.ReferenceNumber).IsUnique();
        entity.HasIndex(x => new { x.Type, x.Status });
        entity.HasIndex(x => x.BeneficiaryProfileId);
        entity.HasIndex(x => x.BeneficiaryGuardianId);

        entity.HasOne(x => x.BeneficiaryProfile)
            .WithMany(x => x.GuardianOperations)
            .HasForeignKey(x => x.BeneficiaryProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.BeneficiaryGuardian)
            .WithMany(x => x.Operations)
            .HasForeignKey(x => x.BeneficiaryGuardianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BeneficiaryUpdateBatchConfigration : IEntityTypeConfiguration<BeneficiaryUpdateBatch>
{
    public void Configure(EntityTypeBuilder<BeneficiaryUpdateBatch> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BatchNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.AssignedTo).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.BatchNumber).IsUnique();
        entity.HasIndex(x => new { x.Kind, x.Status });
        entity.HasIndex(x => x.DueDate);
    }
}

public class BeneficiaryUpdateRequestConfigration : IEntityTypeConfiguration<BeneficiaryUpdateRequest>
{
    public void Configure(EntityTypeBuilder<BeneficiaryUpdateRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestedField).IsRequired().HasMaxLength(120);
        entity.Property(x => x.CurrentValue).HasMaxLength(1000);
        entity.Property(x => x.RequestedValue).HasMaxLength(1000);
        entity.Property(x => x.Reason).HasMaxLength(1000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.BeneficiaryProfileId, x.Status });

        entity.HasOne(x => x.BeneficiaryProfile)
            .WithMany(x => x.UpdateRequests)
            .HasForeignKey(x => x.BeneficiaryProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BeneficiaryEntityConfigration : IEntityTypeConfiguration<BeneficiaryEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryEntity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NameEn).HasMaxLength(200);
        entity.Property(x => x.ContactPerson).HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.City).HasMaxLength(120);
        entity.Property(x => x.Address).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.NameAr).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}
