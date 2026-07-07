using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class MembershipTypeConfigration : IEntityTypeConfiguration<MembershipType>
{
    public void Configure(EntityTypeBuilder<MembershipType> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameEn).HasMaxLength(120);
        entity.Property(x => x.AnnualFee).HasColumnType("decimal(18,2)");
        entity.Property(x => x.VotingWeight).HasColumnType("decimal(18,2)");
        entity.HasIndex(x => x.NameAr).IsUnique();
    }
}

public class MemberProfileConfigration : IEntityTypeConfiguration<MemberProfile>
{
    public void Configure(EntityTypeBuilder<MemberProfile> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MemberNumber).IsRequired().HasMaxLength(40);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.City).HasMaxLength(80);
        entity.Property(x => x.Address).HasMaxLength(300);
        entity.Property(x => x.CumulativePercentage).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.CancellationReason).HasMaxLength(1000);
        entity.Property(x => x.ApplicationUserId).HasMaxLength(450);
        entity.HasIndex(x => x.MemberNumber).IsUnique();
        entity.HasIndex(x => x.NationalId);
        entity.HasIndex(x => x.Email);

        entity.HasOne(x => x.MembershipType)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.MembershipTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class MemberPaymentConfigration : IEntityTypeConfiguration<MemberPayment>
{
    public void Configure(EntityTypeBuilder<MemberPayment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.ReceiptNumber).HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.MemberProfileId, x.DueDate });

        entity.HasOne(x => x.MemberProfile)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.MemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MemberCardConfigration : IEntityTypeConfiguration<MemberCard>
{
    public void Configure(EntityTypeBuilder<MemberCard> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CardNumber).IsRequired().HasMaxLength(60);
        entity.HasIndex(x => x.CardNumber).IsUnique();

        entity.HasOne(x => x.MemberProfile)
            .WithMany(x => x.Cards)
            .HasForeignKey(x => x.MemberProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MemberReportShareConfigration : IEntityTypeConfiguration<MemberReportShare>
{
    public void Configure(EntityTypeBuilder<MemberReportShare> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.Audience).IsRequired().HasMaxLength(80);
    }
}
