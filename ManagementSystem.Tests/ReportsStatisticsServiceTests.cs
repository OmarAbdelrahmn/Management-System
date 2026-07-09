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
}
