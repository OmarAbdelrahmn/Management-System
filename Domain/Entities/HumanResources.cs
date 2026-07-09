using Domain.Auditing;

namespace Domain.Entities;

public enum EmployeeStatus
{
    Active = 0,
    OnLeave = 1,
    Suspended = 2,
    Terminated = 3
}

public enum LeaveRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum AttendanceStatus
{
    Present = 0,
    Late = 1,
    Absent = 2,
    Remote = 3
}

public enum EmployeeAccountType
{
    Employee = 0,
    Volunteer = 1
}

public enum EmployeeDisciplinaryRecordType
{
    Warning = 0,
    Question = 1
}

public enum HumanResourceRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}

public enum PayrollRecordStatus
{
    Draft = 0,
    Reviewed = 1,
    Approved = 2,
    Paid = 3
}

public enum RecruitmentRequestStatus
{
    Requested = 0,
    Announced = 1,
    Received = 2,
    Interviewed = 3,
    Completed = 4,
    Cancelled = 5
}

public enum SafetyRecordStatus
{
    Open = 0,
    InProgress = 1,
    Closed = 2,
    Cancelled = 3
}

public class EmployeeDepartment : IAuditable
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<EmployeeProfile> Employees { get; set; } = new List<EmployeeProfile>();
}

public class JobTitle : IAuditable
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<EmployeeProfile> Employees { get; set; } = new List<EmployeeProfile>();
}

public class EmployeeProfile : IAuditable
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public int DepartmentId { get; set; }
    public int JobTitleId { get; set; }
    public EmployeeAccountType AccountType { get; set; } = EmployeeAccountType.Employee;
    public DateTime HireDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public string? Notes { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeDepartment? Department { get; set; }
    public JobTitle? JobTitle { get; set; }
    public ICollection<EmployeeAttendance> AttendanceRecords { get; set; } = new List<EmployeeAttendance>();
    public ICollection<EmployeeLeaveRequest> LeaveRequests { get; set; } = new List<EmployeeLeaveRequest>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeDisciplinaryRecord> DisciplinaryRecords { get; set; } = new List<EmployeeDisciplinaryRecord>();
    public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
    public ICollection<EmployeeEvaluation> Evaluations { get; set; } = new List<EmployeeEvaluation>();
    public ICollection<EmployeeCardIssue> CardIssues { get; set; } = new List<EmployeeCardIssue>();
    public ICollection<EmployeeLetterRequest> LetterRequests { get; set; } = new List<EmployeeLetterRequest>();
    public ICollection<EmployeePayrollRecord> PayrollRecords { get; set; } = new List<EmployeePayrollRecord>();
    public ICollection<EmployeeAttendanceExcuse> AttendanceExcuses { get; set; } = new List<EmployeeAttendanceExcuse>();
    public ICollection<EmployeeAdministrativeRequest> AdministrativeRequests { get; set; } = new List<EmployeeAdministrativeRequest>();
}

public class EmployeeAttendance : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeLeaveRequest : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public string? Reason { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeDocument : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeDisciplinaryRecord : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public EmployeeDisciplinaryRecordType Type { get; set; } = EmployeeDisciplinaryRecordType.Warning;
    public DateTime RecordDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public HumanResourceRequestStatus Status { get; set; } = HumanResourceRequestStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeLeaveBalance : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public int Year { get; set; } = DateTime.UtcNow.AddHours(3).Year;
    public string LeaveType { get; set; } = string.Empty;
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedDays { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeEvaluation : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 100;
    public string Rating { get; set; } = string.Empty;
    public string? EvaluatorName { get; set; }
    public string? Strengths { get; set; }
    public string? ImprovementAreas { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeCardIssue : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string CardType { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? ExpiresAt { get; set; }
    public HumanResourceRequestStatus Status { get; set; } = HumanResourceRequestStatus.Approved;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeLetterRequest : IAuditable
{
    public int Id { get; set; }
    public int? EmployeeProfileId { get; set; }
    public string LetterType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string Body { get; set; } = string.Empty;
    public HumanResourceRequestStatus Status { get; set; } = HumanResourceRequestStatus.Pending;
    public DateTime? IssuedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeePayrollRecord : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public DateTime PayrollMonth { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollRecordStatus Status { get; set; } = PayrollRecordStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class EmployeeAttendancePolicy : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan WorkStart { get; set; } = new(8, 0, 0);
    public TimeSpan WorkEnd { get; set; } = new(16, 0, 0);
    public int GraceMinutes { get; set; } = 15;
    public string WorkDays { get; set; } = "Sunday,Monday,Tuesday,Wednesday,Thursday";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class EmployeeAttendanceLocation : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int RadiusMeters { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class EmployeeOfficialVacation : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class EmployeeAttendanceExcuse : IAuditable
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public DateTime WorkDate { get; set; }
    public string ExcuseType { get; set; } = string.Empty;
    public TimeSpan? FromTime { get; set; }
    public TimeSpan? ToTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public HumanResourceRequestStatus Status { get; set; } = HumanResourceRequestStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}

public class HrSafetyCategory : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<HrSafetyProcedure> Procedures { get; set; } = new List<HrSafetyProcedure>();
    public ICollection<HrSafetyInspection> Inspections { get; set; } = new List<HrSafetyInspection>();
}

public class HrSafetyProcedure : IAuditable
{
    public int Id { get; set; }
    public int? HrSafetyCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProcedureText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public HrSafetyCategory? HrSafetyCategory { get; set; }
}

public class HrSafetyInspection : IAuditable
{
    public int Id { get; set; }
    public int? HrSafetyCategoryId { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CorrectiveAction { get; set; }
    public SafetyRecordStatus Status { get; set; } = SafetyRecordStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public HrSafetyCategory? HrSafetyCategory { get; set; }
}

public class RecruitmentRequest : IAuditable
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int JobTitleId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public int RequestedCount { get; set; } = 1;
    public string? Justification { get; set; }
    public RecruitmentRequestStatus Status { get; set; } = RecruitmentRequestStatus.Requested;
    public DateTime? AnnouncedAt { get; set; }
    public string? CandidateName { get; set; }
    public string? CandidateMobile { get; set; }
    public string? CandidateEmail { get; set; }
    public DateTime? InterviewAt { get; set; }
    public string? InterviewNotes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeDepartment? Department { get; set; }
    public JobTitle? JobTitle { get; set; }
}

public class EmployeeAdministrativeRequest : IAuditable
{
    public int Id { get; set; }
    public int? EmployeeProfileId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public HumanResourceRequestStatus Status { get; set; } = HumanResourceRequestStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public EmployeeProfile? EmployeeProfile { get; set; }
}
