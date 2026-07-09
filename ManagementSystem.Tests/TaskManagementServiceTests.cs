using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Domain.Auditing;
using Domain.Entities;
using Domain.Identity;

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
