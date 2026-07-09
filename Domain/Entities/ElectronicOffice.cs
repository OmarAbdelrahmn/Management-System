using Domain.Auditing;

namespace Domain.Entities;

public enum OfficeAttendanceType
{
    CheckIn = 0,
    CheckOut = 1
}

public enum OfficeRecordStatus
{
    Pending = 0,
    Approved = 1,
    RejectedWithNote = 2,
    Rejected = 3,
    Completed = 4,
    Canceled = 5
}

public enum OfficeRequestType
{
    Vacation = 0,
    Excuse = 1,
    Finance = 2,
    General = 3,
    VolunteerGroup = 4,
    Purchase = 5,
    Car = 6,
    Evaluation = 7,
    Notification = 8,
    StrategyIndicator = 9,
    Question = 10,
    Warning = 11
}

public enum OfficeTransactionStatus
{
    Pending = 0,
    RequiredFollowUp = 1,
    RejectedWithNote = 2,
    Managed = 3,
    Completed = 4,
    Canceled = 5
}

public enum OfficeLogType
{
    NgoInformation = 0,
    EmployeeProfile = 1,
    EmployeeRecord = 2,
    Tutorial = 3,
    LocalNotification = 4,
    DirectedNotification = 5,
    ReminderNotification = 6,
    VacationCredit = 7,
    AttendanceReport = 8,
    AttendanceRejected = 9,
    MyTasks = 10,
    ProjectTasks = 11,
    StrategyTasks = 12,
    StrategyIndicators = 13,
    MailPreference = 14
}

public class OfficeAttendanceEntry : IAuditable
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public OfficeAttendanceType AttendanceType { get; set; } = OfficeAttendanceType.CheckIn;
    public DateTime AttendanceAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public OfficeRecordStatus Status { get; set; } = OfficeRecordStatus.Approved;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class OfficeReminder : IAuditable
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public OfficeRecordStatus Status { get; set; } = OfficeRecordStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class OfficeAdministrativeRequest : IAuditable
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public OfficeRequestType RequestType { get; set; } = OfficeRequestType.General;
    public string RequestedBy { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public OfficeRecordStatus Status { get; set; } = OfficeRecordStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class OfficeTransaction : IAuditable
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? CurrentStep { get; set; }
    public OfficeTransactionStatus Status { get; set; } = OfficeTransactionStatus.Pending;
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class OfficeLogRecord : IAuditable
{
    public int Id { get; set; }
    public OfficeLogType LogType { get; set; } = OfficeLogType.EmployeeRecord;
    public string Title { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
