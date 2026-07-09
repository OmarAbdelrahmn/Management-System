using Domain.Entities;

namespace Application.Contracts.ElectronicOffice;

public record ElectronicOfficeDashboardResponse(int AttendanceCount, int OpenRemindersCount, int PendingRequestsCount, int ActiveTransactionsCount, int LogRecordsCount);
public record OfficeAttendanceResponse(int Id, string EmployeeName, string AttendanceType, DateTime AttendanceAt, string Status, string? Notes);
public record SaveOfficeAttendanceRequest(string EmployeeName, OfficeAttendanceType AttendanceType, DateTime AttendanceAt, OfficeRecordStatus Status, string? Notes);
public record OfficeReminderResponse(int Id, string OwnerName, string Title, DateTime DueAt, string Status, string? Notes);
public record SaveOfficeReminderRequest(string OwnerName, string Title, DateTime DueAt, OfficeRecordStatus Status, string? Notes);
public record OfficeAdministrativeRequestResponse(int Id, string RequestNumber, string RequestType, string RequestedBy, string Subject, string Status, string? DecisionNotes, DateTime RequestedAt);
public record SaveOfficeAdministrativeRequestRequest(string RequestNumber, OfficeRequestType RequestType, string RequestedBy, string Subject, OfficeRecordStatus Status, string? DecisionNotes, DateTime RequestedAt);
public record DecideOfficeRequestRequest(OfficeRecordStatus Status, string? DecisionNotes);
public record OfficeTransactionResponse(int Id, string TransactionNumber, string Subject, string RequestedBy, string? CurrentStep, string Status, string? Notes, DateTime RequestedAt);
public record SaveOfficeTransactionRequest(string TransactionNumber, string Subject, string RequestedBy, string? CurrentStep, OfficeTransactionStatus Status, string? Notes, DateTime RequestedAt);
public record UpdateOfficeTransactionStatusRequest(OfficeTransactionStatus Status, string? Notes);
public record OfficeLogRecordResponse(int Id, string LogType, string Title, string? Reference, string? Notes, DateTime RecordDate);
public record SaveOfficeLogRecordRequest(OfficeLogType LogType, string Title, string? Reference, string? Notes, DateTime RecordDate);
