using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

public class TaskManagementServiceTests
{
    [Fact]
    public async Task CreateTaskAsync_RecordsCreatedActivity()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, assignee) = await SeedUsersAsync(dbcontext);
        var service = new TaskManagementService(dbcontext, new TestCurrentUserContext(creator.Id));

        var created = await service.CreateTaskAsync(new CreateManagementTaskRequest(
            "مهمة اختبار",
            "وصف",
            assignee.Id,
            new DateTime(2026, 7, 20),
            ManagementTaskPriority.High,
            "General",
            null));
        var activities = await service.GetTaskActivitiesAsync(created.Value.Id);

        Assert.True(created.IsSuccess);
        var activity = Assert.Single(activities.Value);
        Assert.Equal("Created", activity.Type);
        Assert.Equal("New", activity.ToStatus);
        Assert.Equal(assignee.Id, activity.ToAssigneeUserId);
    }

    [Fact]
    public async Task TaskActions_RecordActivityTrail()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, assignee) = await SeedUsersAsync(dbcontext);
        var service = new TaskManagementService(dbcontext, new TestCurrentUserContext(creator.Id));
        var created = await service.CreateTaskAsync(new CreateManagementTaskRequest("مهمة متابعة", null, assignee.Id, null, ManagementTaskPriority.Normal, null, null));

        await service.CompleteTaskAsync(created.Value.Id, new CompleteTaskRequest(100, "تم الإنجاز"));
        await service.DeleteTaskAsync(created.Value.Id, new DeleteTaskRequest("اختبار الحذف"));
        await service.RestoreTaskAsync(created.Value.Id);
        await service.AddTaskCommentAsync(created.Value.Id, new AddTaskCommentRequest("تعليق متابعة"));
        var activities = (await service.GetTaskActivitiesAsync(created.Value.Id)).Value.ToList();

        Assert.Contains(activities, x => x.Type == "Completed" && x.ToStatus == "Completed");
        Assert.Contains(activities, x => x.Type == "Deleted" && x.ToStatus == "Deleted");
        Assert.Contains(activities, x => x.Type == "Restored" && x.ToStatus == "InProgress");
        Assert.Contains(activities, x => x.Type == "Comment" && x.Note == "تعليق متابعة");
    }

    [Fact]
    public async Task RedirectTaskAsync_RecordsAssigneeTransition()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (creator, assignee) = await SeedUsersAsync(dbcontext);
        var newAssignee = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "new@example.com", Email = "new@example.com", FullName = "مكلف جديد" };
        dbcontext.Users.Add(newAssignee);
        await dbcontext.SaveChangesAsync();
        var service = new TaskManagementService(dbcontext, new TestCurrentUserContext(creator.Id));
        var created = await service.CreateTaskAsync(new CreateManagementTaskRequest("مهمة تحويل", null, assignee.Id, null, ManagementTaskPriority.Normal, null, null));

        var redirected = await service.RedirectTaskAsync(created.Value.Id, new RedirectTaskRequest(newAssignee.Id, "إعادة توزيع"));
        var activities = (await service.GetTaskActivitiesAsync(created.Value.Id)).Value.ToList();

        Assert.True(redirected.IsSuccess);
        Assert.Contains(activities, x =>
            x.Type == "Redirected" &&
            x.FromAssigneeUserId == assignee.Id &&
            x.ToAssigneeUserId == newAssignee.Id);
    }

    [Fact]
    public async Task ApprovalDelegation_TracksNewApproverAndEscalatesOnlyOnce()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (requester, approver) = await SeedUsersAsync(dbcontext);
        var delegateUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "delegate@example.com",
            Email = "delegate@example.com",
            FullName = "المفوض إليه"
        };
        dbcontext.Users.Add(delegateUser);
        await dbcontext.SaveChangesAsync();

        var requesterService = new TaskManagementService(dbcontext, new TestCurrentUserContext(requester.Id));
        var route = await requesterService.CreateApprovalRouteAsync(new CreateApprovalRouteRequest("مسار اختبار", "Task", true, 1));
        await requesterService.AddApprovalStepAsync(route.Value.Id, new AddApprovalStepRequest(1, "الموافق", approver.Id));
        var approval = await requesterService.CreateApprovalRequestAsync(new CreateApprovalRequestRequest(route.Value.Id, "طلب اختبار", "Task", 10));

        var approverService = new TaskManagementService(dbcontext, new TestCurrentUserContext(approver.Id));
        var delegated = await approverService.DelegateApprovalRequestAsync(
            approval.Value.Id,
            new DelegateApprovalRequestRequest(approver.Id, delegateUser.Id, "تغطية أثناء الإجازة"));
        var pendingForDelegate = await approverService.GetPendingApprovalRequestsAsync(delegateUser.Id);

        Assert.True(delegated.IsSuccess);
        Assert.Single(pendingForDelegate.Value);
        var delegation = Assert.Single(await dbcontext.ApprovalActions.ToListAsync());
        Assert.Equal(ApprovalActionDecision.Delegated, delegation.Decision);
        Assert.Equal(delegateUser.Id, delegation.DelegatedToUserId);

        var storedApproval = await dbcontext.ApprovalRequests.FindAsync(approval.Value.Id);
        storedApproval!.DueAt = DateTime.UtcNow.AddHours(3).AddMinutes(-1);
        await dbcontext.SaveChangesAsync();

        var firstEscalation = await approverService.EscalateOverdueApprovalRequestsAsync();
        var secondEscalation = await approverService.EscalateOverdueApprovalRequestsAsync();

        Assert.Equal(1, firstEscalation.Value);
        Assert.Equal(0, secondEscalation.Value);
        var notification = Assert.Single(await dbcontext.SystemNotifications.Include(x => x.Recipients).ToListAsync(), x => x.Title == "طلب اعتماد متأخر");
        Assert.Equal(delegateUser.Id, Assert.Single(notification.Recipients).RecipientUserId);
    }

    [Fact]
    public async Task EnsureApprovalRequestForEntityAsync_CreatesOnceForConfiguredRoute()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (requester, approver) = await SeedUsersAsync(dbcontext);
        var service = new TaskManagementService(dbcontext, new TestCurrentUserContext(requester.Id));
        var route = await service.CreateApprovalRouteAsync(new CreateApprovalRouteRequest("اعتماد الإجازات", nameof(EmployeeLeaveRequest), true));
        await service.AddApprovalStepAsync(route.Value.Id, new AddApprovalStepRequest(1, "مدير الموارد البشرية", approver.Id));

        var first = await service.EnsureApprovalRequestForEntityAsync(nameof(EmployeeLeaveRequest), 42, "طلب إجازة");
        var second = await service.EnsureApprovalRequestForEntityAsync(nameof(EmployeeLeaveRequest), 42, "طلب إجازة مكرر");

        Assert.True(first.IsSuccess);
        Assert.NotNull(first.Value);
        Assert.Equal(first.Value!.Id, second.Value!.Id);
        Assert.Equal(1, await dbcontext.ApprovalRequests.CountAsync());
    }

    [Fact]
    public async Task EnsureApprovalRequestForEntityAsync_IsOptionalWhenRouteIsNotConfigured()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var (requester, _) = await SeedUsersAsync(dbcontext);
        var service = new TaskManagementService(dbcontext, new TestCurrentUserContext(requester.Id));

        var result = await service.EnsureApprovalRequestForEntityAsync(nameof(ProgramApproval), 7, "اعتماد برنامج");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Empty(dbcontext.ApprovalRequests);
    }

    private static async Task<(ApplicationUser Creator, ApplicationUser Assignee)> SeedUsersAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var creator = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "creator@example.com", Email = "creator@example.com", FullName = "منشئ المهمة" };
        var assignee = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "assignee@example.com", Email = "assignee@example.com", FullName = "مكلف المهمة" };
        dbcontext.Users.AddRange(creator, assignee);
        await dbcontext.SaveChangesAsync();
        return (creator, assignee);
    }

    private sealed class TestCurrentUserContext(string userId) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = ["Admin"];
    }
}
