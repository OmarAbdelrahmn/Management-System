using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class HumanResourceErrors
{
    public static readonly Error EmployeeNotFound =
        new("HumanResources.EmployeeNotFound", "Employee was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DepartmentNotFound =
        new("HumanResources.DepartmentNotFound", "Department was not found.", StatusCodes.Status404NotFound);

    public static readonly Error JobTitleNotFound =
        new("HumanResources.JobTitleNotFound", "Job title was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LeaveRequestNotFound =
        new("HumanResources.LeaveRequestNotFound", "Leave request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error EmployeeDocumentNotFound =
        new("HumanResources.EmployeeDocumentNotFound", "Employee document was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DisciplinaryRecordNotFound =
        new("HumanResources.DisciplinaryRecordNotFound", "Disciplinary record was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LeaveBalanceNotFound =
        new("HumanResources.LeaveBalanceNotFound", "Leave balance was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LeaveAlreadyDecided =
        new("HumanResources.LeaveAlreadyDecided", "Leave request has already been decided.", StatusCodes.Status409Conflict);

    public static readonly Error InsufficientLeaveBalance =
        new("HumanResources.InsufficientLeaveBalance", "Employee leave balance is insufficient for this request.", StatusCodes.Status409Conflict);

    public static readonly Error EvaluationNotFound =
        new("HumanResources.EvaluationNotFound", "Employee evaluation was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidEvaluationStatusTransition =
        new("HumanResources.InvalidEvaluationStatusTransition", "Employee evaluation status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error EvaluationDecisionNotesRequired =
        new("HumanResources.EvaluationDecisionNotesRequired", "Decision notes are required for rejected employee evaluations.", StatusCodes.Status400BadRequest);

    public static readonly Error CardIssueNotFound =
        new("HumanResources.CardIssueNotFound", "Employee card issue was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CardIssueAlreadyClosed =
        new("HumanResources.CardIssueAlreadyClosed", "Employee card issue is already closed.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidCardIssueStatusTransition =
        new("HumanResources.InvalidCardIssueStatusTransition", "Employee card issue status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error CardIssueDecisionNotesRequired =
        new("HumanResources.CardIssueDecisionNotesRequired", "Decision notes are required for cancelled employee cards.", StatusCodes.Status400BadRequest);

    public static readonly Error LetterRequestNotFound =
        new("HumanResources.LetterRequestNotFound", "Employee letter request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error PayrollRecordNotFound =
        new("HumanResources.PayrollRecordNotFound", "Payroll record was not found.", StatusCodes.Status404NotFound);

    public static readonly Error PayrollAlreadyPaid =
        new("HumanResources.PayrollAlreadyPaid", "Payroll record is already paid.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidPayrollStatusTransition =
        new("HumanResources.InvalidPayrollStatusTransition", "Payroll record status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error AttendancePolicyNotFound =
        new("HumanResources.AttendancePolicyNotFound", "Attendance policy was not found.", StatusCodes.Status404NotFound);

    public static readonly Error AttendanceLocationNotFound =
        new("HumanResources.AttendanceLocationNotFound", "Attendance location was not found.", StatusCodes.Status404NotFound);

    public static readonly Error OfficialVacationNotFound =
        new("HumanResources.OfficialVacationNotFound", "Official vacation was not found.", StatusCodes.Status404NotFound);

    public static readonly Error AttendanceExcuseNotFound =
        new("HumanResources.AttendanceExcuseNotFound", "Attendance excuse was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SafetyCategoryNotFound =
        new("HumanResources.SafetyCategoryNotFound", "Safety category was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SafetyProcedureNotFound =
        new("HumanResources.SafetyProcedureNotFound", "Safety procedure was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SafetyInspectionNotFound =
        new("HumanResources.SafetyInspectionNotFound", "Safety inspection was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RecruitmentRequestNotFound =
        new("HumanResources.RecruitmentRequestNotFound", "Recruitment request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRecruitmentStatusTransition =
        new("HumanResources.InvalidRecruitmentStatusTransition", "Recruitment request status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error RecruitmentCandidateRequired =
        new("HumanResources.RecruitmentCandidateRequired", "Recruitment candidate name and contact information are required before this step.", StatusCodes.Status400BadRequest);

    public static readonly Error RecruitmentInterviewRequired =
        new("HumanResources.RecruitmentInterviewRequired", "Recruitment interview details are required before this step.", StatusCodes.Status400BadRequest);

    public static readonly Error RecruitmentCancellationNotesRequired =
        new("HumanResources.RecruitmentCancellationNotesRequired", "Cancellation notes are required for recruitment requests.", StatusCodes.Status400BadRequest);

    public static readonly Error RecruitmentAlreadyClosed =
        new("HumanResources.RecruitmentAlreadyClosed", "Recruitment request is already completed or cancelled.", StatusCodes.Status409Conflict);

    public static readonly Error RecruitmentNotCompleted =
        new("HumanResources.RecruitmentNotCompleted", "Recruitment request must be completed before creating an employee profile.", StatusCodes.Status400BadRequest);

    public static readonly Error RecruitmentAlreadyConverted =
        new("HumanResources.RecruitmentAlreadyConverted", "Recruitment request has already been converted to an employee profile.", StatusCodes.Status409Conflict);

    public static readonly Error AdministrativeRequestNotFound =
        new("HumanResources.AdministrativeRequestNotFound", "Administrative request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateEmployeeNumber =
        new("HumanResources.DuplicateEmployeeNumber", "Employee number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateAttendance =
        new("HumanResources.DuplicateAttendance", "Attendance is already recorded for this employee and date.", StatusCodes.Status409Conflict);

    public static readonly Error AttendanceRecordNotFound =
        new("HumanResources.AttendanceRecordNotFound", "Attendance record was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateLeaveBalance =
        new("HumanResources.DuplicateLeaveBalance", "Leave balance already exists for this employee, year, and leave type.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateCardNumber =
        new("HumanResources.DuplicateCardNumber", "Card number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRequest =
        new("HumanResources.InvalidRequest", "Human resource request is invalid.", StatusCodes.Status400BadRequest);
}
