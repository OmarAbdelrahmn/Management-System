using Application.Contracts.ReportsStatistics;
using Application.Service.ReportsStatistics;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class ReportsStatisticsServiceTests
{
    [Fact]
    public async Task GetDefinitionsAsync_SeedsAllRafedReportDefinitions()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ReportsStatisticsService(dbcontext);

        var definitions = await service.GetDefinitionsAsync();
        var dashboard = await service.GetDashboardAsync();

        Assert.True(definitions.IsSuccess);
        Assert.Equal(33, definitions.Value.Count());
        Assert.Equal(25, dashboard.Value.ReportDefinitionsCount);
        Assert.Equal(8, dashboard.Value.StatisticDefinitionsCount);
    }

    [Fact]
    public async Task GenerateReportAsync_CreatesRunWithSourceRowCount()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.BeneficiaryProfiles.Add(new BeneficiaryProfile { BeneficiaryNumber = "BEN-1", FullName = "مستفيد", Mobile = "0500000000" });
        await dbcontext.SaveChangesAsync();
        var service = new ReportsStatisticsService(dbcontext);

        var run = await service.GenerateReportAsync(new GenerateSystemReportRequest("report_users", "Table", "{}", "مدير النظام"));
        var runs = await service.GetRunsAsync("report_users");

        Assert.True(run.IsSuccess);
        Assert.Equal(1, run.Value.RowCount);
        Assert.Single(runs.Value);
    }

    [Fact]
    public async Task GenerateReportAsync_AppliesSupportedJsonFilters()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.BeneficiaryProfiles.AddRange(
            new BeneficiaryProfile { BeneficiaryNumber = "BEN-A", FullName = "نشط", City = "الرياض", Status = BeneficiaryStatus.Active },
            new BeneficiaryProfile { BeneficiaryNumber = "BEN-B", FullName = "مؤرشف", City = "جدة", Status = BeneficiaryStatus.Archived });
        dbcontext.BeneficiaryAidRequests.AddRange(
            new BeneficiaryAidRequest { RequestNumber = "AID-A", AidType = "مالية", Description = "داخلي", Amount = 100, Status = AidRequestStatus.CommitteeApproved, IsExternal = false },
            new BeneficiaryAidRequest { RequestNumber = "AID-B", AidType = "مالية", Description = "خارجي", Amount = 100, Status = AidRequestStatus.External, IsExternal = true });
        dbcontext.ExpenseVouchers.AddRange(
            new ExpenseVoucher { ExpenseNumber = "EXP-A", Kind = ExpenseVoucherKind.Generic, Amount = 100, PayeeName = "مورد", Status = AccountingRecordStatus.Approved },
            new ExpenseVoucher { ExpenseNumber = "EXP-B", Kind = ExpenseVoucherKind.PaymentOrder, Amount = 100, PayeeName = "مورد", Status = AccountingRecordStatus.Draft });
        await dbcontext.SaveChangesAsync();
        var service = new ReportsStatisticsService(dbcontext);

        var activeBeneficiaries = await service.GenerateReportAsync(new GenerateSystemReportRequest("report_users", "Table", "{\"status\":\"Active\",\"city\":\"الرياض\"}", "مدير"));
        var externalAid = await service.GenerateReportAsync(new GenerateSystemReportRequest("report_requests", "Table", "{\"isExternal\":true}", "مدير"));
        var approvedExpenses = await service.GenerateReportAsync(new GenerateSystemReportRequest("report_expenses", "Table", "{\"status\":\"Approved\"}", "مدير"));

        Assert.Equal(1, activeBeneficiaries.Value.RowCount);
        Assert.Equal(1, externalAid.Value.RowCount);
        Assert.Equal(1, approvedExpenses.Value.RowCount);
    }

    [Fact]
    public async Task ExportReportAsync_ReturnsFilteredCsvRowsAndRunMetadata()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.BeneficiaryProfiles.AddRange(
            new BeneficiaryProfile { BeneficiaryNumber = "BEN-A", FullName = "نشط", City = "الرياض", Status = BeneficiaryStatus.Active },
            new BeneficiaryProfile { BeneficiaryNumber = "BEN-B", FullName = "مؤرشف", City = "جدة", Status = BeneficiaryStatus.Archived });
        await dbcontext.SaveChangesAsync();
        var service = new ReportsStatisticsService(dbcontext);

        var export = await service.ExportReportAsync(new GenerateSystemReportRequest("report_users", "Csv", "{\"status\":\"Active\"}", "مدير"));
        var runs = await service.GetRunsAsync("report_users");

        Assert.True(export.IsSuccess);
        Assert.Equal(1, export.Value.Run.RowCount);
        Assert.EndsWith(".csv", export.Value.FileName);
        Assert.Contains("BeneficiaryNumber,FullName", export.Value.Content);
        Assert.Contains("BEN-A", export.Value.Content);
        Assert.DoesNotContain("BEN-B", export.Value.Content);
        Assert.Single(runs.Value);
    }

    [Fact]
    public async Task ArchiveRunsAsync_HidesArchivedRunsByDefaultAndPreservesAuditHistory()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        dbcontext.BeneficiaryProfiles.Add(new BeneficiaryProfile { BeneficiaryNumber = "BEN-A", FullName = "نشط", City = "الرياض", Status = BeneficiaryStatus.Active });
        await dbcontext.SaveChangesAsync();
        var service = new ReportsStatisticsService(dbcontext);

        var userRun = await service.GenerateReportAsync(new GenerateSystemReportRequest("report_users", "Table", "{}", "مدير"));
        var financeRun = await service.GenerateReportAsync(new GenerateSystemReportRequest("statistics_finance", "Dashboard", "{}", "مدير"));
        var archived = await service.ArchiveRunsAsync(new ArchiveSystemReportRunsRequest("report_users", DateTime.UtcNow.AddHours(3).AddMinutes(1), "مشرف التقارير"));
        var visibleUserRuns = await service.GetRunsAsync("report_users");
        var allUserRuns = await service.GetRunsAsync("report_users", includeArchived: true);
        var visibleRuns = await service.GetRunsAsync();
        var dashboard = await service.GetDashboardAsync();

        Assert.True(userRun.IsSuccess);
        Assert.True(financeRun.IsSuccess);
        Assert.True(archived.IsSuccess);
        Assert.Equal(1, archived.Value.ArchivedCount);
        Assert.Empty(visibleUserRuns.Value);
        Assert.Equal("Archived", Assert.Single(allUserRuns.Value).Status);
        Assert.Equal(financeRun.Value.Id, Assert.Single(visibleRuns.Value).Id);
        Assert.Equal(1, dashboard.Value.RunsCount);
    }
}
