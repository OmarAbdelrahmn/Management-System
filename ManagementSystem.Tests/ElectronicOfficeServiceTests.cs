using Application.Contracts.ElectronicOffice;
using Application.Service.ElectronicOffice;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class ElectronicOfficeServiceTests
{
    [Fact]
    public async Task AttendanceAndReminders_UpdateDashboard()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ElectronicOfficeService(dbcontext);

        var attendance = await service.SaveAttendanceAsync(new SaveOfficeAttendanceRequest("موظف", OfficeAttendanceType.CheckIn, DateTime.UtcNow.AddHours(3), OfficeRecordStatus.Approved, null));
        var reminder = await service.SaveReminderAsync(null, new SaveOfficeReminderRequest("موظف", "مذكرة", DateTime.UtcNow.AddHours(3).AddDays(1), OfficeRecordStatus.Pending, null));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(attendance.IsSuccess);
        Assert.True(reminder.IsSuccess);
        Assert.Equal(1, dashboard.Value.AttendanceCount);
        Assert.Equal(1, dashboard.Value.OpenRemindersCount);
    }

    [Fact]
    public async Task RequestsAndTransactions_CanChangeStatus()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ElectronicOfficeService(dbcontext);

        var request = await service.SaveRequestAsync(null, new SaveOfficeAdministrativeRequestRequest("REQ-1", OfficeRequestType.Vacation, "موظف", "إجازة", OfficeRecordStatus.Pending, null, DateTime.UtcNow.AddHours(3)));
        var transaction = await service.SaveTransactionAsync(null, new SaveOfficeTransactionRequest("TR-1", "معاملة", "موظف", "المراجعة", OfficeTransactionStatus.RequiredFollowUp, null, DateTime.UtcNow.AddHours(3)));
        var approve = await service.DecideRequestAsync(request.Value.Id, new DecideOfficeRequestRequest(OfficeRecordStatus.Approved, "معتمد"));
        var complete = await service.UpdateTransactionStatusAsync(transaction.Value.Id, new UpdateOfficeTransactionStatusRequest(OfficeTransactionStatus.Completed, "مكتملة"));

        Assert.True(approve.IsSuccess);
        Assert.True(complete.IsSuccess);
        Assert.Single((await service.GetRequestsAsync(status: OfficeRecordStatus.Approved)).Value);
        Assert.Single((await service.GetTransactionsAsync(OfficeTransactionStatus.Completed)).Value);
    }

    [Fact]
    public async Task LogRecords_AreSavedByType()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new ElectronicOfficeService(dbcontext);

        var log = await service.SaveLogRecordAsync(null, new SaveOfficeLogRecordRequest(OfficeLogType.MailPreference, "تفضيل البريد", "mail", "تفعيل", DateTime.UtcNow.AddHours(3)));
        var records = await service.GetLogRecordsAsync(OfficeLogType.MailPreference);

        Assert.True(log.IsSuccess);
        Assert.Single(records.Value);
    }
}
