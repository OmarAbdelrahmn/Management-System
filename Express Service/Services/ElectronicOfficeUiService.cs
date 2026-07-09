using Application.Contracts.ElectronicOffice;
using Application.Service.ElectronicOffice;
using Domain.Entities;

namespace Express_Service.Services;

public class ElectronicOfficeUiService(IElectronicOfficeService service)
{
    public async Task<ElectronicOfficeDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }
    public async Task<List<OfficeAttendanceResponse>> GetAttendanceAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetAttendanceAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveAttendanceAsync(SaveOfficeAttendanceRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveAttendanceAsync(request, cancellationToken), "تم حفظ سجل الحضور.");
    public async Task<List<OfficeReminderResponse>> GetRemindersAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetRemindersAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveReminderAsync(int? id, SaveOfficeReminderRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveReminderAsync(id, request, cancellationToken), "تم حفظ المذكرة.");
    public async Task<List<OfficeAdministrativeRequestResponse>> GetRequestsAsync(OfficeRequestType? type = null, OfficeRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetRequestsAsync(type, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveRequestAsync(int? id, SaveOfficeAdministrativeRequestRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveRequestAsync(id, request, cancellationToken), "تم حفظ الطلب الإداري.");
    public async Task<(bool Success, string Message)> DecideRequestAsync(int id, OfficeRecordStatus status, string? note = null, CancellationToken cancellationToken = default) => ToUi(await service.DecideRequestAsync(id, new DecideOfficeRequestRequest(status, note), cancellationToken), "تم تحديث الطلب.");
    public async Task<List<OfficeTransactionResponse>> GetTransactionsAsync(OfficeTransactionStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetTransactionsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveTransactionAsync(int? id, SaveOfficeTransactionRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveTransactionAsync(id, request, cancellationToken), "تم حفظ المعاملة.");
    public async Task<(bool Success, string Message)> UpdateTransactionStatusAsync(int id, OfficeTransactionStatus status, string? notes = null, CancellationToken cancellationToken = default) => ToUi(await service.UpdateTransactionStatusAsync(id, new UpdateOfficeTransactionStatusRequest(status, notes), cancellationToken), "تم تحديث المعاملة.");
    public async Task<List<OfficeLogRecordResponse>> GetLogRecordsAsync(OfficeLogType? type = null, CancellationToken cancellationToken = default) => ToList(await service.GetLogRecordsAsync(type, cancellationToken));
    public async Task<(bool Success, string Message)> SaveLogRecordAsync(int? id, SaveOfficeLogRecordRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveLogRecordAsync(id, request, cancellationToken), "تم حفظ السجل.");

    private static (bool Success, string Message) ToUi(Application.Abstraction.Result result, string successMessage) => result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) => result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) => result.IsSuccess ? result.Value.ToList() : [];
}
