using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class FinancialSupporterConfigration : IEntityTypeConfiguration<FinancialSupporter>
{
    public void Configure(EntityTypeBuilder<FinancialSupporter> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Category).HasMaxLength(120);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.NationalIdOrRegistrationNo).HasMaxLength(80);
        entity.Property(x => x.PreferredContactChannel).IsRequired().HasMaxLength(50);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.SupporterType });
        entity.HasIndex(x => x.Mobile);
    }
}

public class FundraisingOpportunityConfigration : IEntityTypeConfiguration<FundraisingOpportunity>
{
    public void Configure(EntityTypeBuilder<FundraisingOpportunity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(80);
        entity.Property(x => x.TargetAmount).HasPrecision(18, 2);
        entity.Property(x => x.CurrentAmount).HasPrecision(18, 2);
        entity.Property(x => x.ExternalUrl).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(1500);
        entity.HasIndex(x => new { x.OpportunityType, x.Status });
        entity.HasIndex(x => x.ReferenceNumber);
    }
}

public class DonationContributionConfigration : IEntityTypeConfiguration<DonationContribution>
{
    public void Configure(EntityTypeBuilder<DonationContribution> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.SourceChannel).IsRequired().HasMaxLength(80);
        entity.Property(x => x.PaymentMethod).HasMaxLength(80);
        entity.Property(x => x.TransactionReference).HasMaxLength(120);
        entity.Property(x => x.GiftRecipientName).HasMaxLength(200);
        entity.Property(x => x.CertificateNumber).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.DonationDate });
        entity.HasIndex(x => x.SourceChannel);
        entity.HasOne(x => x.FinancialSupporter).WithMany(x => x.Contributions).HasForeignKey(x => x.FinancialSupporterId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.FundraisingOpportunity).WithMany(x => x.Contributions).HasForeignKey(x => x.FundraisingOpportunityId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class DonationContributionActivityConfigration : IEntityTypeConfiguration<DonationContributionActivity>
{
    public void Configure(EntityTypeBuilder<DonationContributionActivity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.DonationContributionId, x.OccurredAt });
        entity.HasIndex(x => x.Type);
        entity.HasOne(x => x.DonationContribution).WithMany(x => x.Activities).HasForeignKey(x => x.DonationContributionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DigitalMarketingCampaignConfigration : IEntityTypeConfiguration<DigitalMarketingCampaign>
{
    public void Configure(EntityTypeBuilder<DigitalMarketingCampaign> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Budget).HasPrecision(18, 2);
        entity.Property(x => x.TargetAudience).HasMaxLength(500);
        entity.Property(x => x.LandingPageUrl).HasMaxLength(500);
        entity.Property(x => x.DonationsAmount).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Channel, x.Status });
    }
}

public class AbandonedDonationCartConfigration : IEntityTypeConfiguration<AbandonedDonationCart>
{
    public void Configure(EntityTypeBuilder<AbandonedDonationCart> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SupporterName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.FollowUpNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.CartDate });
        entity.HasOne(x => x.FundraisingOpportunity).WithMany(x => x.AbandonedCarts).HasForeignKey(x => x.FundraisingOpportunityId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class EndowmentAssetConfigration : IEntityTypeConfiguration<EndowmentAsset>
{
    public void Configure(EntityTypeBuilder<EndowmentAsset> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.EndowmentNumber).HasMaxLength(80);
        entity.Property(x => x.AssetType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.EstimatedValue).HasPrecision(18, 2);
        entity.Property(x => x.AnnualReturnEstimate).HasPrecision(18, 2);
        entity.Property(x => x.ManagerName).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.AssetType });
        entity.HasIndex(x => x.EndowmentNumber);
    }
}

public class EndowmentContractConfigration : IEntityTypeConfiguration<EndowmentContract>
{
    public void Configure(EntityTypeBuilder<EndowmentContract> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ContractNumber).IsRequired().HasMaxLength(120);
        entity.Property(x => x.LesseeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.AnnualAmount).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.EndDate });
        entity.HasOne(x => x.EndowmentAsset).WithMany(x => x.Contracts).HasForeignKey(x => x.EndowmentAssetId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EndowmentInvoiceConfigration : IEntityTypeConfiguration<EndowmentInvoice>
{
    public void Configure(EntityTypeBuilder<EndowmentInvoice> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.DueDate });
        entity.HasOne(x => x.EndowmentAsset).WithMany(x => x.Invoices).HasForeignKey(x => x.EndowmentAssetId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.EndowmentContract).WithMany(x => x.Invoices).HasForeignKey(x => x.EndowmentContractId).OnDelete(DeleteBehavior.Restrict);
    }
}
