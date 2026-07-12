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

    [Fact]
    public async Task SaveActivityAsync_UpdatesLinkedCaseWorkflow()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new EvaluationFollowUpService(dbcontext);
        var followUpCase = await service.SaveCaseAsync(null, new SaveFollowUpCaseRequest(FollowUpSubjectType.AidRequest, "طلب إعانة", "AID-1", "المتابع", new DateTime(2026, 7, 8), new DateTime(2026, 7, 30), FollowUpPriority.Urgent, FollowUpCaseStatus.Requested, null, null, null));

        await service.SaveActivityAsync(null, new SaveFollowUpActivityRequest(
            followUpCase.Value.Id,
            FollowUpSubjectType.AidRequest,
            "طلب إعانة",
            "AID-1",
            new DateTime(2026, 7, 9),
            "زيارة",
            "تمت الزيارة",
            null,
            "المتابع",
            true,
            new DateTime(2026, 7, 12)));
        var running = (await service.GetCasesAsync(status: FollowUpCaseStatus.Running)).Value.Single();

        await service.SaveActivityAsync(null, new SaveFollowUpActivityRequest(
            followUpCase.Value.Id,
            FollowUpSubjectType.AidRequest,
            "طلب إعانة",
            "AID-1",
            new DateTime(2026, 7, 13),
            "إغلاق",
            "تم استكمال المتابعة",
            "يوصى بالاعتماد",
            "المتابع",
            false,
            null));
        var pendingApproval = (await service.GetCasesAsync(status: FollowUpCaseStatus.PendingApproval)).Value.Single();

        Assert.Equal(new DateTime(2026, 7, 12), running.DueDate);
        Assert.Equal("يوصى بالاعتماد", pendingApproval.CompletionSummary);
    }

    [Fact]
    public async Task ApprovalWorkflow_RequiresPendingApprovalSummaryAndResolvedNextActions()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new EvaluationFollowUpService(dbcontext);
        var followUpCase = await service.SaveCaseAsync(null, new SaveFollowUpCaseRequest(FollowUpSubjectType.Beneficiary, "مستفيد", "BEN-2", "المتابع", new DateTime(2026, 7, 8), new DateTime(2026, 7, 20), FollowUpPriority.High, FollowUpCaseStatus.Requested, null, null, null));

        await service.SaveActivityAsync(null, new SaveFollowUpActivityRequest(
            followUpCase.Value.Id,
            FollowUpSubjectType.Beneficiary,
            "مستفيد",
            "BEN-2",
            new DateTime(2026, 7, 9),
            "زيارة أولية",
            "تحتاج متابعة إضافية",
            null,
            "المتابع",
            true,
            new DateTime(2026, 7, 12)));
        var directApproval = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.Approved, "اعتماد مباشر"));
        var pendingApproval = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.PendingApproval, "ملخص أولي"));
        var blockedApproval = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.Approved, "اعتماد قبل إغلاق الإجراء"));

        await service.SaveActivityAsync(null, new SaveFollowUpActivityRequest(
            followUpCase.Value.Id,
            FollowUpSubjectType.Beneficiary,
            "مستفيد",
            "BEN-2",
            new DateTime(2026, 7, 13),
            "إجراء ختامي",
            "تم استكمال المتابعة",
            "يوصى باعتماد الحالة",
            "المتابع",
            false,
            null));
        var nextActions = await service.GetActivitiesAsync(followUpCase.Value.Id, nextActionsOnly: true);
        var approved = await service.UpdateCaseStatusAsync(followUpCase.Value.Id, new UpdateFollowUpCaseStatusRequest(FollowUpCaseStatus.Approved, "تم الاعتماد"));
        var dashboard = await service.GetDashboardAsync();

        Assert.False(directApproval.IsSuccess);
        Assert.True(pendingApproval.IsSuccess);
        Assert.False(blockedApproval.IsSuccess);
        Assert.Empty(nextActions.Value);
        Assert.True(approved.IsSuccess);
        Assert.Equal("Approved", approved.Value.Status);
        Assert.Equal(1, dashboard.Value.CompletedCount);
    }
}
