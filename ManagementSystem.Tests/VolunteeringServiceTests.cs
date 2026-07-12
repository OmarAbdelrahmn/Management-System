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
    public async Task ConvertRequestToVolunteerAsync_CreatesAccountLinksOpportunityAndRespectsCapacity()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new VolunteeringService(dbcontext);
        var opportunity = await service.SaveOpportunityAsync(null, new SaveVolunteerOpportunityRequest("OPP-CNV-1", "تنظيم معرض", "وصف", "التطوع", DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(3).AddDays(1), 1, VolunteerOpportunityStatus.Open, null, null));
        var request = await service.SaveRequestAsync(null, new SaveVolunteerRequestRequest("REQ-CNV-1", VolunteerRequestSource.External, "متقدم جديد", "0500000010", "تنظيم معرض", VolunteerRequestStatus.Submitted, DateTime.UtcNow.AddHours(3), null, null, null, null));
        var secondRequest = await service.SaveRequestAsync(null, new SaveVolunteerRequestRequest("REQ-CNV-2", VolunteerRequestSource.External, "متقدم آخر", "0500000011", "تنظيم معرض", VolunteerRequestStatus.Submitted, DateTime.UtcNow.AddHours(3), null, null, null, null));

        var converted = await service.ConvertRequestToVolunteerAsync(request.Value.Id, new ConvertVolunteerRequestRequest(null, opportunity.Value.Id, "استقبال وتنظيم", DateTime.UtcNow.AddHours(3), "جاهز", "معتمد"));
        var full = await service.ConvertRequestToVolunteerAsync(secondRequest.Value.Id, new ConvertVolunteerRequestRequest(null, opportunity.Value.Id, "تنظيم", DateTime.UtcNow.AddHours(3), null, "معتمد"));
        var users = await service.GetUsersAsync(VolunteerUserStatus.Active);
        var approvedRequests = await service.GetRequestsAsync(status: VolunteerRequestStatus.Approved);
        var opportunities = await service.GetOpportunitiesAsync();

        Assert.True(converted.IsSuccess);
        Assert.False(full.IsSuccess);
        Assert.StartsWith("VOL-", converted.Value.VolunteerUser.VolunteerNumber);
        Assert.Equal("Approved", converted.Value.Request.Status);
        Assert.Equal(opportunity.Value.Id, converted.Value.Request.VolunteerOpportunityId);
        Assert.Single(users.Value);
        Assert.Single(approvedRequests.Value);
        Assert.Equal(1, Assert.Single(opportunities.Value).RequestsCount);
    }

    [Fact]
    public async Task OpportunityWorkflow_SavesTasksReportsAndAttendance()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new VolunteeringService(dbcontext);
        var unapprovedUser = await service.SaveUserAsync(null, new SaveVolunteerUserRequest("VOL-2", "متطوع غير مرتبط", null, "0500000002", null, null, VolunteerUserStatus.Active, DateTime.UtcNow.AddHours(3), null));
        var opportunity = await service.SaveOpportunityAsync(null, new SaveVolunteerOpportunityRequest("OPP-1", "تنظيم فعالية", "وصف", "العلاقات العامة", DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(3).AddDays(1), 20, VolunteerOpportunityStatus.Open, null, null));
        var request = await service.SaveRequestAsync(null, new SaveVolunteerRequestRequest("REQ-ATT-1", VolunteerRequestSource.External, "متطوع حضور", "0500000090", "تنظيم فعالية", VolunteerRequestStatus.Submitted, DateTime.UtcNow.AddHours(3), null, null, null, opportunity.Value.Id));

        var task = await service.SaveTaskAsync(null, new SaveVolunteerOpportunityTaskRequest(opportunity.Value.Id, "تجهيز القاعة", "المشرف", DateTime.UtcNow.AddHours(3).AddDays(1), VolunteerTaskStatus.Open, null));
        var reportWithOpenTask = await service.SaveOpportunityReportAsync(opportunity.Value.Id, new SaveVolunteerOpportunityReportRequest("إجراءات", "تقرير", VolunteerOpportunityStatus.Completed));
        var completedTask = await service.SaveTaskAsync(task.Value.Id, new SaveVolunteerOpportunityTaskRequest(opportunity.Value.Id, "تجهيز القاعة", "المشرف", DateTime.UtcNow.AddHours(3).AddDays(1), VolunteerTaskStatus.Completed, null));
        var reportWithoutAttendance = await service.SaveOpportunityReportAsync(opportunity.Value.Id, new SaveVolunteerOpportunityReportRequest("إجراءات", "تقرير", VolunteerOpportunityStatus.Completed));
        var unapprovedAttendance = await service.SaveAttendanceAsync(null, new SaveVolunteerAttendanceRequest(opportunity.Value.Id, unapprovedUser.Value.Id, DateTime.UtcNow.AddHours(3), 3.5m, VolunteerAttendanceStatus.Present, null));
        var converted = await service.ConvertRequestToVolunteerAsync(request.Value.Id, new ConvertVolunteerRequestRequest(null, opportunity.Value.Id, "تنظيم", DateTime.UtcNow.AddHours(3), null, "معتمد للحضور"));
        var attendance = await service.SaveAttendanceAsync(null, new SaveVolunteerAttendanceRequest(opportunity.Value.Id, converted.Value.VolunteerUser.Id, DateTime.UtcNow.AddHours(3), 3.5m, VolunteerAttendanceStatus.Present, null));
        var duplicateAttendance = await service.SaveAttendanceAsync(null, new SaveVolunteerAttendanceRequest(opportunity.Value.Id, converted.Value.VolunteerUser.Id, DateTime.UtcNow.AddHours(3), 1m, VolunteerAttendanceStatus.Present, null));
        var report = await service.SaveOpportunityReportAsync(opportunity.Value.Id, new SaveVolunteerOpportunityReportRequest("تمت الإجراءات", "تقرير مختصر", VolunteerOpportunityStatus.Completed));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(task.IsSuccess);
        Assert.False(reportWithOpenTask.IsSuccess);
        Assert.True(completedTask.IsSuccess);
        Assert.False(reportWithoutAttendance.IsSuccess);
        Assert.False(unapprovedAttendance.IsSuccess);
        Assert.True(converted.IsSuccess);
        Assert.True(attendance.IsSuccess);
        Assert.False(duplicateAttendance.IsSuccess);
        Assert.True(report.IsSuccess);
        Assert.Equal(1, dashboard.Value.CompletedTasksCount);
        Assert.Equal(3.5m, dashboard.Value.AttendanceHours);
    }
}
