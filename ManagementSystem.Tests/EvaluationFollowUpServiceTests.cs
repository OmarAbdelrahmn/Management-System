using Application.Contracts.EvaluationFollowUp;
using Application.Service.EvaluationFollowUp;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class EvaluationFollowUpServiceTests
{
    [Fact]
    public async Task CaseWorkflow_CreateRunCompleteRejectAndDashboard()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new EvaluationFollowUpService(dbcontext);

        var followUpCase = await service.SaveCaseAsync(null, new SaveFollowUpCaseRequest(
            FollowUpSubjectType.Beneficiary,
            "مستفيد أول",
            "BEN-1",
            "الباحث",
            new DateTime(2026, 7, 8),
            new DateTime(2026, 7, 20),
            FollowUpPriority.High,
            FollowUpCaseStatus.Requested,
            null,
            null,
            null));
        var running = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.Running, "بدأت المتابعة"));
        var pendingApproval = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.PendingApproval, "انتهت المتابعة"));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(followUpCase.IsSuccess);
        Assert.StartsWith("FUP-2026-", followUpCase.Value.CaseNumber);
        Assert.Equal("Running", running.Value.Status);
        Assert.Equal("PendingApproval", pendingApproval.Value.Status);
        Assert.Equal(1, dashboard.Value.PendingApprovalCount);
    }

    [Fact]
    public async Task ActivityRecords_LinkToCasesAndFilterNextActions()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new EvaluationFollowUpService(dbcontext);
        var followUpCase = await service.SaveCaseAsync(null, new SaveFollowUpCaseRequest(FollowUpSubjectType.Supporter, "داعم", "SUP-1", "المتابع", new DateTime(2026, 7, 8), null, FollowUpPriority.Normal, FollowUpCaseStatus.Running, null, null, null));

        var activity = await service.SaveActivityAsync(null, new SaveFollowUpActivityRequest(
            followUpCase.Value.Id,
            FollowUpSubjectType.Supporter,
            "داعم",
            "SUP-1",
            new DateTime(2026, 7, 9),
            "اتصال",
            "تم التواصل مع الداعم",
            "طلب إعادة الاتصال",
            "المتابع",
            true,
            new DateTime(2026, 7, 12)));
        var nextActions = await service.GetActivitiesAsync(nextActionsOnly: true);

        Assert.True(activity.IsSuccess);
        Assert.Equal(followUpCase.Value.CaseNumber, activity.Value.CaseNumber);
        Assert.Single(nextActions.Value);
    }
}
