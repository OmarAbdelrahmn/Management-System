using Application.Contracts.Volunteering;
using Application.Service.Volunteering;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class VolunteeringServiceTests
{
    [Fact]
    public async Task SaveUserAsync_CreatesVolunteerAccountAndDashboardCounts()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new VolunteeringService(dbcontext);

        var saved = await service.SaveUserAsync(null, new SaveVolunteerUserRequest("VOL-1", "متطوع تجريبي", "100", "0500000000", "v@example.com", "تنظيم", VolunteerUserStatus.Active, DateTime.UtcNow.AddHours(3), null));
        var users = await service.GetUsersAsync(VolunteerUserStatus.Active);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(saved.IsSuccess);
        Assert.Single(users.Value);
        Assert.Equal(1, dashboard.Value.UsersCount);
        Assert.Equal(1, dashboard.Value.ActiveUsersCount);
    }

    [Fact]
    public async Task SaveRequestAsync_TracksInternalAndExternalStatuses()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new VolunteeringService(dbcontext);

        var request = await service.SaveRequestAsync(null, new SaveVolunteerRequestRequest("REQ-1", VolunteerRequestSource.External, "متقدم خارجي", "0500000001", "فرصة إعلامية", VolunteerRequestStatus.Submitted, DateTime.UtcNow.AddHours(3), null, null, null, null));
        var update = await service.UpdateRequestStatusAsync(request.Value.Id, new UpdateVolunteerRequestStatusRequest(VolunteerRequestStatus.Approved, "مناسب"));
        var approved = await service.GetRequestsAsync(status: VolunteerRequestStatus.Approved);

        Assert.True(request.IsSuccess);
        Assert.True(update.IsSuccess);
        Assert.Single(approved.Value);
        Assert.Equal(VolunteerRequestSource.External.ToString(), approved.Value.Single().Source);
    }

    [Fact]
    public async Task OpportunityWorkflow_SavesTasksReportsAndAttendance()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new VolunteeringService(dbcontext);
        var user = await service.SaveUserAsync(null, new SaveVolunteerUserRequest("VOL-2", "متطوع حضور", null, "0500000002", null, null, VolunteerUserStatus.Active, DateTime.UtcNow.AddHours(3), null));
        var opportunity = await service.SaveOpportunityAsync(null, new SaveVolunteerOpportunityRequest("OPP-1", "تنظيم فعالية", "وصف", "العلاقات العامة", DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(3).AddDays(1), 20, VolunteerOpportunityStatus.Open, null, null));

        var task = await service.SaveTaskAsync(null, new SaveVolunteerOpportunityTaskRequest(opportunity.Value.Id, "تجهيز القاعة", "المشرف", DateTime.UtcNow.AddHours(3).AddDays(1), VolunteerTaskStatus.Completed, null));
        var attendance = await service.SaveAttendanceAsync(null, new SaveVolunteerAttendanceRequest(opportunity.Value.Id, user.Value.Id, DateTime.UtcNow.AddHours(3), 3.5m, VolunteerAttendanceStatus.Present, null));
        var report = await service.SaveOpportunityReportAsync(opportunity.Value.Id, new SaveVolunteerOpportunityReportRequest("تمت الإجراءات", "تقرير مختصر", VolunteerOpportunityStatus.Completed));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(task.IsSuccess);
        Assert.True(attendance.IsSuccess);
        Assert.True(report.IsSuccess);
        Assert.Equal(1, dashboard.Value.CompletedTasksCount);
        Assert.Equal(3.5m, dashboard.Value.AttendanceHours);
    }
}
