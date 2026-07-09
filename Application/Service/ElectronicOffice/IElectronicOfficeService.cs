using Application.Abstraction;
using Application.Contracts.ElectronicOffice;
using Domain.Entities;

namespace Application.Service.ElectronicOffice;

public interface IElectronicOfficeService
{
    Task<Result<ElectronicOfficeDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OfficeAttendanceResponse>>> GetAttendanceAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<OfficeAttendanceResponse>> SaveAttendanceAsync(SaveOfficeAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OfficeReminderResponse>>> GetRemindersAsync(OfficeRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<OfficeReminderResponse>> SaveReminderAsync(int? id, SaveOfficeReminderRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OfficeAdministrativeRequestResponse>>> GetRequestsAsync(OfficeRequestType? type = null, OfficeRecordStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<OfficeAdministrativeRequestResponse>> SaveRequestAsync(int? id, SaveOfficeAdministrativeRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result> DecideRequestAsync(int id, DecideOfficeRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OfficeTransactionResponse>>> GetTransactionsAsync(OfficeTransactionStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<OfficeTransactionResponse>> SaveTransactionAsync(int? id, SaveOfficeTransactionRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateTransactionStatusAsync(int id, UpdateOfficeTransactionStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OfficeLogRecordResponse>>> GetLogRecordsAsync(OfficeLogType? type = null, CancellationToken cancellationToken = default);
    Task<Result<OfficeLogRecordResponse>> SaveLogRecordAsync(int? id, SaveOfficeLogRecordRequest request, CancellationToken cancellationToken = default);
}
