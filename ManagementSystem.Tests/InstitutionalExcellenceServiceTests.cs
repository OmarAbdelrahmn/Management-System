using Application.Contracts.InstitutionalExcellence;
using Application.Service.InstitutionalExcellence;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class InstitutionalExcellenceServiceTests
{
    [Fact]
    public async Task PerformanceMeasures_SaveAndUpdateDashboardAchievement()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new InstitutionalExcellenceService(dbcontext);

        var measure = await service.SavePerformanceMeasureAsync(null, new SavePerformanceMeasureRequest(
            "PPS-1",
            "مؤشر رضا المستفيدين",
            PerformanceMeasureType.Quantitative,
            100,
            85,
            "%",
            "2026",
            ExcellenceRecordStatus.Active,
            null));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(measure.IsSuccess);
        Assert.Equal(85, measure.Value.AchievementPercent);
        Assert.Equal(1, dashboard.Value.PerformanceMeasuresCount);
        Assert.Equal(85, dashboard.Value.AveragePerformanceAchievement);
    }

    [Fact]
    public async Task GovernanceCycleCriteriaAttachmentsAndTasks_UpdateReport()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new InstitutionalExcellenceService(dbcontext);

        var cycle = await service.SaveGovernanceCycleAsync(null, new SaveGovernanceCycleRequest("حوكمة 2025", 2025, true, ExcellenceRecordStatus.Active, "خارطة الطريق"));
        var criterion = await service.SaveGovernanceCriterionAsync(null, new SaveGovernanceCriterionRequest(
            cycle.Value.Id,
            "G-1",
            "توفر السياسات",
            2,
            100,
            80,
            GovernanceCriterionStatus.Met,
            "السياسات محدثة",
            "تم التحقق",
            1200));
        var attachment = await service.SaveGovernanceAttachmentAsync(null, new SaveGovernanceAttachmentRequest(criterion.Value.Id, "policy.pdf", "/files/policy.pdf", null, new DateTime(2026, 7, 8)));
        var task = await service.SaveGovernanceTaskAsync(null, new SaveGovernanceTaskRequest(cycle.Value.Id, "مراجعة الأدلة", "أمين الحوكمة", new DateTime(2026, 7, 15), GovernanceTaskStatus.InProgress, 60, null));
        var report = await service.GetGovernanceReportAsync(cycle.Value.Id);

        Assert.True(cycle.IsSuccess);
        Assert.True(attachment.IsSuccess);
        Assert.True(task.IsSuccess);
        Assert.Equal(80, report.Value.Score);
        Assert.Equal(1, report.Value.AttachmentsCount);
        Assert.Equal(1, report.Value.OpenTasksCount);
    }

    [Fact]
    public async Task StrategicPlanStructureIndicatorsAndVariables_SaveAndFetch()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new InstitutionalExcellenceService(dbcontext);

        var plan = await service.SaveStrategicPlanAsync(null, new SaveStrategicPlanRequest("خطة 2026", new DateTime(2026, 1, 1), new DateTime(2028, 12, 31), ExcellenceRecordStatus.Active, "رؤية", "رسالة", null));
        var perspective = await service.SaveStrategicPerspectiveAsync(null, new SaveStrategicPerspectiveRequest(plan.Value.Id, "المستفيدون", 1));
        var goal = await service.SaveStrategicGoalAsync(null, new SaveStrategicGoalRequest(perspective.Value.Id, "تعظيم الأثر", "وصف", "رؤية 2030", "SDG", 1));
        var mainIndicator = await service.SaveStrategicIndicatorAsync(null, new SaveStrategicIndicatorRequest(plan.Value.Id, goal.Value.Id, null, StrategicIndicatorKind.Main, "مؤشر الأثر", 100, 70, "%", "مالك", null, null, StrategicIndicatorStatus.Active, null));
        var subIndicator = await service.SaveStrategicIndicatorAsync(null, new SaveStrategicIndicatorRequest(plan.Value.Id, goal.Value.Id, mainIndicator.Value.Id, StrategicIndicatorKind.Sub, "مؤشر فرعي", 100, 60, "%", "مالك", null, null, StrategicIndicatorStatus.Active, null));
        var variable = await service.SaveStrategicVariableAsync(null, new SaveStrategicVariableRequest(plan.Value.Id, "عدد البرامج", 12, "Programs", true));
        var fetched = await service.FetchAutomatedStrategicVariablesAsync(plan.Value.Id);
        var indicators = await service.GetStrategicIndicatorsAsync(plan.Value.Id);

        Assert.True(subIndicator.IsSuccess);
        Assert.True(variable.IsSuccess);
        Assert.NotNull(Assert.Single(fetched.Value).LastFetchedAt);
        Assert.Equal(2, indicators.Value.Count());
    }
}
