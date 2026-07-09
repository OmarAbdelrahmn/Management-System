using Application.Contracts.FinancialDevelopment;
using Application.Service.FinancialDevelopment;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class FinancialDevelopmentServiceTests
{
    [Fact]
    public async Task SupporterOpportunityAndConfirmedDonation_UpdateTotalsAndReports()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);

        var supporter = await service.SaveSupporterAsync(null, new SaveFinancialSupporterRequest(
            "داعم رئيسي",
            FinancialSupporterType.Company,
            "ذهبي",
            "0500000000",
            "donor@example.com",
            "7000000000",
            "WhatsApp",
            FinancialSupporterStatus.Active,
            null));
        var opportunity = await service.SaveOpportunityAsync(null, new SaveFundraisingOpportunityRequest(
            "حملة مشروع الأسر",
            FundraisingOpportunityType.Project,
            "PRJ-1",
            10000,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31),
            FundraisingOpportunityStatus.Active,
            "https://example.test/donate",
            null));

        var contribution = await service.SaveContributionAsync(null, new SaveDonationContributionRequest(
            supporter.Value.Id,
            opportunity.Value.Id,
            1500,
            new DateTime(2026, 7, 8),
            "Website",
            "Card",
            "TX-1",
            true,
            "مهدى إليه",
            "CERT-1",
            DonationContributionStatus.Confirmed,
            null));
        var opportunities = await service.GetOpportunitiesAsync();
        var report = await service.GetDonationReportAsync(new DateTime(2026, 7, 1), new DateTime(2026, 7, 31));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(supporter.IsSuccess);
        Assert.True(contribution.IsSuccess);
        Assert.Equal(1500, Assert.Single(opportunities.Value).CurrentAmount);
        Assert.Equal(1500, report.Value.TotalConfirmedAmount);
        Assert.Equal(1, report.Value.GiftCount);
        Assert.Equal(1, report.Value.CertificateCount);
        Assert.Equal(1, report.Value.UniqueSupportersCount);
        Assert.Equal(1500, dashboard.Value.ConfirmedDonationsAmount);
    }

    [Fact]
    public async Task DigitalMarketingAndAbandonedCarts_SaveOperationalRecords()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);
        var opportunity = await service.SaveOpportunityAsync(null, new SaveFundraisingOpportunityRequest("فرصة عامة", FundraisingOpportunityType.General, null, 5000, null, null, FundraisingOpportunityStatus.Active, null, null));

        var campaign = await service.SaveDigitalCampaignAsync(null, new SaveDigitalMarketingCampaignRequest(
            "حملة رمضان",
            DigitalMarketingChannel.SocialMedia,
            1200,
            "الداعمون السابقون",
            "https://example.test/ramadan",
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 10),
            DigitalMarketingCampaignStatus.Running,
            30,
            4,
            2200,
            null));
        var cart = await service.SaveAbandonedCartAsync(null, new SaveAbandonedDonationCartRequest(
            opportunity.Value.Id,
            "داعم لم يكمل",
            "0501111111",
            300,
            new DateTime(2026, 7, 8),
            AbandonedDonationCartStatus.Open,
            "متابعة هاتفية"));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(campaign.IsSuccess);
        Assert.Equal("Running", campaign.Value.Status);
        Assert.True(cart.IsSuccess);
        Assert.Equal(1, dashboard.Value.ActiveCampaignsCount);
        Assert.Equal(1, dashboard.Value.OpenAbandonedCartsCount);
    }

    [Fact]
    public async Task EndowmentContractsAndInvoices_SaveAndFilterDueSoon()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);

        var endowment = await service.SaveEndowmentAsync(null, new SaveEndowmentAssetRequest(
            "وقف تجاري",
            "AWQ-1",
            "RealEstate",
            500000,
            40000,
            EndowmentAssetStatus.Active,
            "مدير الوقف",
            null));
        var contract = await service.SaveEndowmentContractAsync(null, new SaveEndowmentContractRequest(
            endowment.Value.Id,
            "CNT-1",
            "مستأجر",
            new DateTime(2026, 7, 1),
            new DateTime(2027, 7, 1),
            40000,
            EndowmentContractStatus.Active,
            null));
        var invoice = await service.SaveEndowmentInvoiceAsync(null, new SaveEndowmentInvoiceRequest(
            endowment.Value.Id,
            contract.Value.Id,
            "INV-1",
            DateTime.UtcNow.AddHours(3).Date.AddDays(10),
            10000,
            0,
            EndowmentInvoiceStatus.Due,
            null));
        var dueSoon = await service.GetEndowmentInvoicesAsync(null, true);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(endowment.IsSuccess);
        Assert.True(contract.IsSuccess);
        Assert.True(invoice.IsSuccess);
        Assert.Single(dueSoon.Value);
        Assert.Equal(1, dashboard.Value.ActiveEndowmentsCount);
        Assert.Equal(1, dashboard.Value.DueEndowmentInvoicesCount);
    }
}
