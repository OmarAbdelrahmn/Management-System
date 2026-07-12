using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class BeneficiaryAidRequestConfigration : IEntityTypeConfiguration<BeneficiaryAidRequest>
{
    public void Configure(EntityTypeBuilder<BeneficiaryAidRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.AidType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Description).IsRequired().HasMaxLength(3000);
        entity.Property(x => x.SocialResearchNotes).HasMaxLength(2000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.Status, x.IsExternal });
        entity.HasOne(x => x.BeneficiaryProfile).WithMany().HasForeignKey(x => x.BeneficiaryProfileId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.BeneficiaryEntity).WithMany().HasForeignKey(x => x.BeneficiaryEntityId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class BeneficiaryPaymentOrderConfigration : IEntityTypeConfiguration<BeneficiaryPaymentOrder>
{
    public void Configure(EntityTypeBuilder<BeneficiaryPaymentOrder> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.OrderNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.ItemDescription).IsRequired().HasMaxLength(2000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.OrderNumber).IsUnique();
        entity.HasIndex(x => new { x.OrderType, x.Status });
        entity.HasOne(x => x.BeneficiaryAidRequest).WithMany().HasForeignKey(x => x.BeneficiaryAidRequestId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.EntitySupportRequest).WithMany().HasForeignKey(x => x.EntitySupportRequestId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.BeneficiaryProfile).WithMany().HasForeignKey(x => x.BeneficiaryProfileId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class SponsorConfigration : IEntityTypeConfiguration<Sponsor>
{
    public void Configure(EntityTypeBuilder<Sponsor> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
    }
}

public class SponsorshipRequirementConfigration : IEntityTypeConfiguration<SponsorshipRequirement>
{
    public void Configure(EntityTypeBuilder<SponsorshipRequirement> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Frequency).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
    }
}

public class SponsorshipRecordConfigration : IEntityTypeConfiguration<SponsorshipRecord>
{
    public void Configure(EntityTypeBuilder<SponsorshipRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.SponsorId, x.Status });
        entity.HasOne(x => x.Sponsor).WithMany(x => x.SponsorshipRecords).HasForeignKey(x => x.SponsorId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.BeneficiaryProfile).WithMany().HasForeignKey(x => x.BeneficiaryProfileId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.SponsorshipRequirement).WithMany().HasForeignKey(x => x.SponsorshipRequirementId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class SponsorshipPaymentConfigration : IEntityTypeConfiguration<SponsorshipPayment>
{
    public void Configure(EntityTypeBuilder<SponsorshipPayment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.SponsorshipRecordId, x.Status, x.DueDate });
        entity.HasOne(x => x.SponsorshipRecord).WithMany(x => x.Payments).HasForeignKey(x => x.SponsorshipRecordId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EntitySupportRequestConfigration : IEntityTypeConfiguration<EntitySupportRequest>
{
    public void Configure(EntityTypeBuilder<EntitySupportRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequesterName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.SupportType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.IsExternal });
        entity.HasOne(x => x.BeneficiaryEntity).WithMany().HasForeignKey(x => x.BeneficiaryEntityId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CouponRequestConfigration : IEntityTypeConfiguration<CouponRequest>
{
    public void Configure(EntityTypeBuilder<CouponRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CouponType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
        entity.HasOne(x => x.BeneficiaryProfile).WithMany().HasForeignKey(x => x.BeneficiaryProfileId).OnDelete(DeleteBehavior.SetNull);
    }
}
