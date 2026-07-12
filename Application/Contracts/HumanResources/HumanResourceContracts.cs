using Domain.Entities;

namespace Application.Contracts.HumanResources;

public record EmployeeDepartmentResponse(
    int Id,
    string NameAr,
    string? NameEn,
    bool IsActive);

public record JobTitleResponse(
    int Id,
    string NameAr,
    string? NameEn,
    bool IsActive);

public record UpsertLookupRequest(
    string NameAr,
    string? NameEn,
    bool IsActive = true);

public record EmployeeResponse(
    int Id,
    string EmployeeNumber,
    string FullName,
    string? NationalId,
    string? Email,
    string? Mobile,
    int DepartmentId,
    string DepartmentName,
    int JobTitleId,
    string JobTitleName,
    DateTime HireDate,
    string Status,
    decimal BasicSalary,
    decimal Allowances,
    decimal TotalSalary,
    string? Notes,
    DateTime? TerminatedAt,
    string? TerminationReason,
    DateTime CreatedAt,
    string AccountType);

public record CreateEmployeeRequest(
    string FullName,
    int DepartmentId,
    int JobTitleId,
    string? EmployeeNumber,
    string? NationalId,
    string? Email,
    string? Mobile,
    DateTime? HireDate,
    decimal BasicSalary,
    decimal Allowances,
    string? Notes,
    EmployeeAccountType AccountType = EmployeeAccountType.Employee);

public record UpdateEmployeeRequest(
    string FullName,
    int DepartmentId,
    int JobTitleId,
    string? NationalId,
    string? Email,
    string? Mobile,
    DateTime HireDate,
    EmployeeStatus Status,
    decimal BasicSalary,
    decimal Allowances,
    string? Notes,
    EmployeeAccountType AccountType = EmployeeAccountType.Employee);

public record EmployeeSearchRequest(
    string? Search,
    EmployeeStatus? Status,
    int? DepartmentId,
    int? JobTitleId,
    EmployeeAccountType? AccountType = null);

public record TerminateEmployeeRequest(string Reason);

public record EmployeeAttendanceResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    DateTime WorkDate,
    TimeSpan? CheckIn,
    TimeSpan? CheckOut,
    string Status,
    string? Notes);

public record RecordEmployeeAttendanceRequest(
    int EmployeeProfileId,
    DateTime WorkDate,
    TimeSpan? CheckIn,
    TimeSpan? CheckOut,
    AttendanceStatus Status,
    string? Notes);

public record EmployeeLeaveRequestResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    string LeaveType,
    DateTime StartsAt,
    DateTime EndsAt,
    int Days,
    string Status,
    string? Reason,
    string? DecisionNotes,
    DateTime? DecidedAt,
    DateTime CreatedAt);

public record CreateEmployeeLeaveRequest(
    int EmployeeProfileId,
    string LeaveType,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Reason);

public record DecideEmployeeLeaveRequest(
    bool Approved,
    string? Notes);

public record EmployeeDocumentResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    string Title,
    string DocumentType,
    string? FilePath,
    DateTime? ExpiresAt,
    string? Notes);

public record AddEmployeeDocumentRequest(
    int EmployeeProfileId,
    string Title,
    string DocumentType,
    string? FilePath,
    DateTime? ExpiresAt,
    string? Notes);

public record HumanResourceDashboardResponse(
    int EmployeesCount,
    int ActiveEmployeesCount,
    int PendingLeaveRequestsCount,
    int TodayAttendanceCount,
    int ExpiringDocumentsCount,
    decimal MonthlyPayrollTotal,
    int PendingAttendanceExcusesCount,
    int PendingAdministrativeRequestsCount,
    int PendingEvaluationsCount,
    int OpenSafetyInspectionsCount,
    int OpenRecruitmentRequestsCount,
    int DraftPayrollRecordsCount,
    int ReviewedPayrollRecordsCount,
    int ApprovedPayrollRecordsCount,
    decimal ApprovedPayrollTotal,
    int RecentActivityCount);

public record EmployeeDisciplinaryRecordResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    string Type,
    DateTime RecordDate,
    string Title,
    string Reason,
    string? ActionTaken,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt);

public record CreateEmployeeDisciplinaryRecordRequest(
    int EmployeeProfileId,
    EmployeeDisciplinaryRecordType Type,
    DateTime? RecordDate,
    string Title,
    string Reason,
    string? ActionTaken);

public record DecideHumanResourceItemRequest(
    HumanResourceRequestStatus Status,
    string? Notes);

public record EmployeeLeaveBalanceResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    int Year,
    string LeaveType,
    decimal EntitledDays,
    decimal UsedDays,
    decimal CarriedDays,
    decimal RemainingDays,
    string? Notes);

public record SaveEmployeeLeaveBalanceRequest(
    int EmployeeProfileId,
    int Year,
    string LeaveType,
    decimal EntitledDays,
    decimal UsedDays,
    decimal CarriedDays,
    string? Notes);

public record EmployeeEvaluationResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Score,
    decimal MaxScore,
    decimal Percentage,
    string Rating,
    string? EvaluatorName,
    string? Strengths,
    string? ImprovementAreas,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt,
    string? Notes);

public record SaveEmployeeEvaluationRequest(
    int EmployeeProfileId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Score,
    decimal MaxScore,
    string Rating,
    string? EvaluatorName,
    string? Strengths,
    string? ImprovementAreas,
    string? Notes,
    EmployeeEvaluationStatus Status = EmployeeEvaluationStatus.Draft);

public record DecideEmployeeEvaluationRequest(
    EmployeeEvaluationStatus Status,
    string? DecisionNotes);

public record EmployeeCardIssueResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    string CardType,
    string CardNumber,
    DateTime IssuedAt,
    DateTime? ExpiresAt,
    string Status,
    string? Notes);

public record IssueEmployeeCardRequest(
    int EmployeeProfileId,
    string CardType,
    string? CardNumber,
    DateTime? IssuedAt,
    DateTime? ExpiresAt,
    string? Notes);

public record DecideEmployeeCardIssueRequest(
    HumanResourceRequestStatus Status,
    string? Notes);

public record EmployeeLetterRequestResponse(
    int Id,
    int? EmployeeProfileId,
    string? EmployeeName,
    string LetterType,
    string Subject,
    string? Purpose,
    string Body,
    string Status,
    DateTime? IssuedAt,
    string? DecisionNotes);

public record SaveEmployeeLetterRequest(
    int? EmployeeProfileId,
    string LetterType,
    string Subject,
    string? Purpose,
    string Body);

public record EmployeePayrollRecordResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    DateTime PayrollMonth,
    decimal BasicSalary,
    decimal Allowances,
    decimal Deductions,
    decimal NetSalary,
    string Status,
    string? Notes,
    string? DecisionNotes,
    DateTime? ReviewedAt,
    DateTime? ApprovedAt,
    DateTime? PaidAt);

public record GeneratePayrollPreviewRequest(
    DateTime PayrollMonth,
    decimal DefaultDeductions,
    string? Notes);

public record DecidePayrollRecordRequest(
    PayrollRecordStatus Status,
    string? Notes);

public record AttendancePolicyResponse(
    int Id,
    string Name,
    TimeSpan WorkStart,
    TimeSpan WorkEnd,
    int GraceMinutes,
    string WorkDays,
    bool IsDefault,
    bool IsActive);

public record SaveAttendancePolicyRequest(
    string Name,
    TimeSpan WorkStart,
    TimeSpan WorkEnd,
    int GraceMinutes,
    string WorkDays,
    bool IsDefault,
    bool IsActive);

public record AttendanceLocationResponse(
    int Id,
    string Name,
    decimal? Latitude,
    decimal? Longitude,
    int RadiusMeters,
    bool IsActive,
    string? Notes);

public record SaveAttendanceLocationRequest(
    string Name,
    decimal? Latitude,
    decimal? Longitude,
    int RadiusMeters,
    bool IsActive,
    string? Notes);

public record OfficialVacationResponse(
    int Id,
    string Name,
    DateTime StartsAt,
    DateTime EndsAt,
    int Days,
    bool IsRecurring,
    string? Notes);

public record SaveOfficialVacationRequest(
    string Name,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsRecurring,
    string? Notes);

public record AttendanceExcuseResponse(
    int Id,
    int EmployeeProfileId,
    string EmployeeName,
    DateTime WorkDate,
    string ExcuseType,
    TimeSpan? FromTime,
    TimeSpan? ToTime,
    string Reason,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt);

public record CreateAttendanceExcuseRequest(
    int EmployeeProfileId,
    DateTime WorkDate,
    string ExcuseType,
    TimeSpan? FromTime,
    TimeSpan? ToTime,
    string Reason);

public record CurrentPresenceResponse(
    int EmployeeProfileId,
    string EmployeeName,
    DateTime WorkDate,
    TimeSpan? CheckIn,
    TimeSpan? CheckOut,
    string Status,
    string? Notes);

public record SafetyCategoryResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive);

public record SaveSafetyCategoryRequest(
    string Name,
    string? Description,
    bool IsActive);

public record SafetyProcedureResponse(
    int Id,
    int? HrSafetyCategoryId,
    string? CategoryName,
    string Title,
    string ProcedureText,
    bool IsActive);

public record SaveSafetyProcedureRequest(
    int? HrSafetyCategoryId,
    string Title,
    string ProcedureText,
    bool IsActive);

public record SafetyInspectionResponse(
    int Id,
    int? HrSafetyCategoryId,
    string? CategoryName,
    DateTime InspectionDate,
    string Location,
    string Description,
    string? CorrectiveAction,
    string Status);

public record SaveSafetyInspectionRequest(
    int? HrSafetyCategoryId,
    DateTime InspectionDate,
    string Location,
    string Description,
    string? CorrectiveAction,
    SafetyRecordStatus Status);

public record RecruitmentRequestResponse(
    int Id,
    int DepartmentId,
    string DepartmentName,
    int JobTitleId,
    string JobTitleName,
    string RequestTitle,
    int RequestedCount,
    string? Justification,
    string Status,
    DateTime? AnnouncedAt,
    string? CandidateName,
    string? CandidateMobile,
    string? CandidateEmail,
    DateTime? InterviewAt,
    string? InterviewNotes,
    DateTime? CompletedAt,
    int? ConvertedEmployeeProfileId,
    string? ConvertedEmployeeName,
    string? Notes);

public record SaveRecruitmentRequest(
    int DepartmentId,
    int JobTitleId,
    string RequestTitle,
    int RequestedCount,
    string? Justification,
    string? CandidateName,
    string? CandidateMobile,
    string? CandidateEmail,
    DateTime? InterviewAt,
    string? InterviewNotes,
    string? Notes);

public record UpdateRecruitmentStatusRequest(
    RecruitmentRequestStatus Status,
    DateTime? AnnouncedAt,
    DateTime? CompletedAt,
    string? Notes,
    DateTime? InterviewAt = null);

public record ConvertRecruitmentToEmployeeRequest(
    string? EmployeeNumber,
    string? NationalId,
    DateTime? HireDate,
    decimal BasicSalary,
    decimal Allowances,
    string? Notes);

public record EmployeeAdministrativeRequestResponse(
    int Id,
    int? EmployeeProfileId,
    string? EmployeeName,
    string RequestType,
    string Title,
    string Details,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt);

public record CreateEmployeeAdministrativeRequest(
    int? EmployeeProfileId,
    string RequestType,
    string Title,
    string Details);

public record HumanResourceActivityResponse(
    int Id,
    string EntityType,
    int EntityId,
    int? EmployeeProfileId,
    string? EmployeeName,
    string Action,
    string Title,
    string? FromStatus,
    string? ToStatus,
    string? Notes,
    DateTime OccurredAt);
