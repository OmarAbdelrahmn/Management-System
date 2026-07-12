using Application.Contracts.FinancialDevelopment;
using Application.Service.FinancialDevelopment;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            DonationContributionStatus.Pending,
            null));
        var confirmed = await service.UpdateContributionStatusAsync(contribution.Value.Id, new UpdateDonationContributionStatusRequest(DonationContributionStatus.Confirmed, "تم التحقق من الدفع"));
        var opportunities = await service.GetOpportunitiesAsync();
        var report = await service.GetDonationReportAsync(new DateTime(2026, 7, 1), new DateTime(2026, 7, 31));
        var dashboard = await service.GetDashboardAsync();
        var activities = (await service.GetContributionActivitiesAsync(contribution.Value.Id)).Value.ToList();

        Assert.True(supporter.IsSuccess);
        Assert.True(contribution.IsSuccess);
        Assert.True(confirmed.IsSuccess);
        Assert.Equal(1500, Assert.Single(opportunities.Value).CurrentAmount);
        Assert.Equal(1500, report.Value.TotalConfirmedAmount);
        Assert.Equal(1, report.Value.GiftCount);
        Assert.Equal(1, report.Value.CertificateCount);
        Assert.Equal(1, report.Value.UniqueSupportersCount);
        Assert.Equal(1500, dashboard.Value.ConfirmedDonationsAmount);
        Assert.Contains(activities, x => x.Type == "Created" && x.ToStatus == "Pending");
        Assert.Contains(activities, x => x.Type == "StatusChanged" && x.FromStatus == "Pending" && x.ToStatus == "Confirmed");

        var refunded = await service.UpdateContributionStatusAsync(contribution.Value.Id, new UpdateDonationContributionStatusRequest(DonationContributionStatus.Refunded, "استرجاع"));
        var opportunityAfterRefund = await service.GetOpportunitiesAsync();

        Assert.True(refunded.IsSuccess);
        Assert.Equal(0, Assert.Single(opportunityAfterRefund.Value).CurrentAmount);
    }

    [Fact]
    public async Task CompleteOpportunityAsync_RequiresTargetAmountAndMarksCompleted()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);
        var opportunity = await service.SaveOpportunityAsync(null, new SaveFundraisingOpportunityRequest(
            "حملة مكتملة",
            FundraisingOpportunityType.General,
            "CMP-1",
            1000,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31),
            FundraisingOpportunityStatus.Active,
            null,
            null));

        await service.SaveContributionAsync(null, new SaveDonationContributionRequest(
            null,
            opportunity.Value.Id,
            700,
            new DateTime(2026, 7, 9),
            "Website",
            "Card",
            "TARGET-1",
            false,
            null,
            null,
            DonationContributionStatus.Confirmed,
            null));
        var earlyCompletion = await service.CompleteOpportunityAsync(opportunity.Value.Id, new CompleteFundraisingOpportunityRequest("محاولة مبكرة"));

        await service.SaveContributionAsync(null, new SaveDonationContributionRequest(
            null,
            opportunity.Value.Id,
            300,
            new DateTime(2026, 7, 10),
            "Website",
            "Card",
            "TARGET-2",
            false,
            null,
            null,
            DonationContributionStatus.Confirmed,
            null));
        var completed = await service.CompleteOpportunityAsync(opportunity.Value.Id, new CompleteFundraisingOpportunityRequest("بلغ المستهدف"));
        var opportunities = await service.GetOpportunitiesAsync();

        Assert.False(earlyCompletion.IsSuccess);
        Assert.True(completed.IsSuccess);
        Assert.Equal("Completed", completed.Value.Status);
        Assert.Equal(1000, completed.Value.CurrentAmount);
        Assert.Contains("بلغ المستهدف", completed.Value.Notes);
        Assert.Equal("Completed", Assert.Single(opportunities.Value).Status);
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
    public async Task RecordDigitalCampaignDonation_CreatesContributionAndUpdatesCampaignTotals()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);
        var supporter = await service.SaveSupporterAsync(null, new SaveFinancialSupporterRequest("داعم حملة", FinancialSupporterType.Individual, null, "0502222222", null, null, "WhatsApp", FinancialSupporterStatus.Active, null));
        var opportunity = await service.SaveOpportunityAsync(null, new SaveFundraisingOpportunityRequest("فرصة حملة", FundraisingOpportunityType.General, null, 5000, null, null, FundraisingOpportunityStatus.Active, null, null));
        var campaign = await service.SaveDigitalCampaignAsync(null, new SaveDigitalMarketingCampaignRequest("حملة مرتبطة", DigitalMarketingChannel.SocialMedia, 1000, "الداعمون", "https://example.test/campaign", new DateTime(2026, 7, 1), null, DigitalMarketingCampaignStatus.Running, 2, 0, 0, null));
        var pausedCampaign = await service.SaveDigitalCampaignAsync(null, new SaveDigitalMarketingCampaignRequest("حملة متوقفة", DigitalMarketingChannel.Email, 1000, null, null, null, null, DigitalMarketingCampaignStatus.Paused, 0, 0, 0, null));

        var recorded = await service.RecordDigitalCampaignDonationAsync(campaign.Value.Id, new RecordDigitalCampaignDonationRequest(
            supporter.Value.Id,
            opportunity.Value.Id,
            750,
            new DateTime(2026, 7, 9),
            "Card",
            "DM-TRX-1",
            DonationContributionStatus.Confirmed,
            true,
            "تبرع من صفحة الهبوط"));
        var blocked = await service.RecordDigitalCampaignDonationAsync(pausedCampaign.Value.Id, new RecordDigitalCampaignDonationRequest(
            supporter.Value.Id,
            opportunity.Value.Id,
            100,
            new DateTime(2026, 7, 10),
            "Card",
            "DM-TRX-2",
            DonationContributionStatus.Confirmed,
            true,
            null));
        var opportunities = await service.GetOpportunitiesAsync();
        var contributions = await service.GetContributionsAsync();
        var campaigns = await service.GetDigitalCampaignsAsync();

        Assert.True(recorded.IsSuccess);
        Assert.Equal("DigitalMarketing:SocialMedia", recorded.Value.Contribution.SourceChannel);
        Assert.Equal("Confirmed", recorded.Value.Contribution.Status);
        Assert.Equal(3, recorded.Value.Campaign.LeadsCount);
        Assert.Equal(1, recorded.Value.Campaign.DonationsCount);
        Assert.Equal(750, recorded.Value.Campaign.DonationsAmount);
        Assert.False(blocked.IsSuccess);
        Assert.Equal(750, Assert.Single(opportunities.Value).CurrentAmount);
        Assert.Single(contributions.Value);
        Assert.Equal(0, campaigns.Value.Single(x => x.Id == pausedCampaign.Value.Id).DonationsAmount);
    }

    [Fact]
    public async Task RecoverAbandonedCartAsync_CreatesContributionAndUpdatesOpportunity()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);
        var opportunity = await service.SaveOpportunityAsync(null, new SaveFundraisingOpportunityRequest("فرصة عامة", FundraisingOpportunityType.General, null, 5000, null, null, FundraisingOpportunityStatus.Active, null, null));
        var cart = await service.SaveAbandonedCartAsync(null, new SaveAbandonedDonationCartRequest(opportunity.Value.Id, "داعم مسترد", "0501111111", 300, new DateTime(2026, 7, 8), AbandonedDonationCartStatus.Open, "اتصال"));

        var recovered = await service.RecoverAbandonedCartAsync(cart.Value.Id, new RecoverAbandonedDonationCartRequest(null, new DateTime(2026, 7, 9), "Card", "TX-REC-1", DonationContributionStatus.Confirmed, "تم الاسترداد"));
        var carts = await service.GetAbandonedCartsAsync(AbandonedDonationCartStatus.Recovered);
        var opportunities = await service.GetOpportunitiesAsync();
        var dashboard = await service.GetDashboardAsync();

        Assert.True(recovered.IsSuccess);
        Assert.Equal("Recovered", recovered.Value.Cart.Status);
        Assert.Equal("Confirmed", recovered.Value.Contribution.Status);
        Assert.Equal("RecoveredCart", recovered.Value.Contribution.SourceChannel);
        Assert.Single(carts.Value);
        Assert.Equal(300, Assert.Single(opportunities.Value).CurrentAmount);
        Assert.Equal(0, dashboard.Value.OpenAbandonedCartsCount);
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

    [Fact]
    public async Task PayEndowmentInvoiceAsync_CreatesReceiptsAndClosesWhenFullyPaid()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new FinancialDevelopmentService(dbcontext);

        var endowment = await service.SaveEndowmentAsync(null, new SaveEndowmentAssetRequest(
            "وقف تجاري",
            "AWQ-2",
            "RealEstate",
            500000,
            40000,
            EndowmentAssetStatus.Active,
            "مدير الوقف",
            null));
        var contract = await service.SaveEndowmentContractAsync(null, new SaveEndowmentContractRequest(
            endowment.Value.Id,
            "CNT-2",
            "شركة مستأجرة",
            new DateTime(2026, 7, 1),
            new DateTime(2027, 7, 1),
            40000,
            EndowmentContractStatus.Active,
            null));
        var invoice = await service.SaveEndowmentInvoiceAsync(null, new SaveEndowmentInvoiceRequest(
            endowment.Value.Id,
            contract.Value.Id,
            "INV-2",
            new DateTime(2026, 7, 20),
            10000,
            0,
            EndowmentInvoiceStatus.Due,
            null));

        var partialPayment = await service.PayEndowmentInvoiceAsync(invoice.Value.Id, new PayEndowmentInvoiceRequest(
            4000,
            new DateTime(2026, 7, 10),
            AccountingRecordStatus.Posted,
            "BankTransfer",
            "TRX-END-1",
            "دفعة جزئية"));
        var overPayment = await service.PayEndowmentInvoiceAsync(invoice.Value.Id, new PayEndowmentInvoiceRequest(
            7000,
            new DateTime(2026, 7, 11),
            AccountingRecordStatus.Posted,
            "BankTransfer",
            "TRX-END-OVER",
            null));
        var finalPayment = await service.PayEndowmentInvoiceAsync(invoice.Value.Id, new PayEndowmentInvoiceRequest(
            6000,
            new DateTime(2026, 7, 12),
            AccountingRecordStatus.Approved,
            "BankTransfer",
            "TRX-END-2",
            "إغلاق الدفعة"));
        var duplicatePayment = await service.PayEndowmentInvoiceAsync(invoice.Value.Id, new PayEndowmentInvoiceRequest(
            1,
            new DateTime(2026, 7, 13),
            AccountingRecordStatus.Posted,
            "Cash",
            null,
            null));
        var receipts = await dbcontext.ReceiptVouchers.OrderBy(x => x.ReceiptDate).ToListAsync();
        var dashboard = await service.GetDashboardAsync();

        Assert.True(partialPayment.IsSuccess);
        Assert.Equal(6000, partialPayment.Value.RemainingAmount);
        Assert.False(partialPayment.Value.IsFullyPaid);
        Assert.Equal("Due", partialPayment.Value.Invoice.Status);
        Assert.Equal("Investment", partialPayment.Value.Receipt.Kind);
        Assert.Equal("شركة مستأجرة", partialPayment.Value.Receipt.PayerName);
        Assert.Equal("TRX-END-1", partialPayment.Value.Receipt.ReferenceNumber);
        Assert.False(overPayment.IsSuccess);
        Assert.True(finalPayment.IsSuccess);
        Assert.Equal(0, finalPayment.Value.RemainingAmount);
        Assert.True(finalPayment.Value.IsFullyPaid);
        Assert.Equal("Paid", finalPayment.Value.Invoice.Status);
        Assert.False(duplicatePayment.IsSuccess);
        Assert.Equal(2, receipts.Count);
        Assert.All(receipts, receipt => Assert.Equal(ReceiptVoucherKind.Investment, receipt.Kind));
        Assert.Equal(0, dashboard.Value.DueEndowmentInvoicesCount);
    }
}
