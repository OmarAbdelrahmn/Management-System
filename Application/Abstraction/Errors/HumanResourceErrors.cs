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

    public static readonly Error DisciplinaryRecordNotFound =
        new("HumanResources.DisciplinaryRecordNotFound", "Disciplinary record was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LeaveBalanceNotFound =
        new("HumanResources.LeaveBalanceNotFound", "Leave balance was not found.", StatusCodes.Status404NotFound);

    public static readonly Error EvaluationNotFound =
        new("HumanResources.EvaluationNotFound", "Employee evaluation was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CardIssueNotFound =
        new("HumanResources.CardIssueNotFound", "Employee card issue was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LetterRequestNotFound =
        new("HumanResources.LetterRequestNotFound", "Employee letter request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error PayrollRecordNotFound =
        new("HumanResources.PayrollRecordNotFound", "Payroll record was not found.", StatusCodes.Status404NotFound);

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

    public static readonly Error AdministrativeRequestNotFound =
        new("HumanResources.AdministrativeRequestNotFound", "Administrative request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateEmployeeNumber =
        new("HumanResources.DuplicateEmployeeNumber", "Employee number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateAttendance =
        new("HumanResources.DuplicateAttendance", "Attendance is already recorded for this employee and date.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateLeaveBalance =
        new("HumanResources.DuplicateLeaveBalance", "Leave balance already exists for this employee, year, and leave type.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateCardNumber =
        new("HumanResources.DuplicateCardNumber", "Card number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRequest =
        new("HumanResources.InvalidRequest", "Human resource request is invalid.", StatusCodes.Status400BadRequest);
}
