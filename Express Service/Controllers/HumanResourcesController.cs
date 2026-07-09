using Application.Contracts.HumanResources;
using Application.Service.HumanResources;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class HumanResourcesController(IHumanResourceService humanResources) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("departments")]
    public async Task<IActionResult> Departments(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetDepartmentsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] UpsertLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveDepartmentAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("departments/{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpsertLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveDepartmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("job-titles")]
    public async Task<IActionResult> JobTitles(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetJobTitlesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("job-titles")]
    public async Task<IActionResult> CreateJobTitle([FromBody] UpsertLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveJobTitleAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("job-titles/{id:int}")]
    public async Task<IActionResult> UpdateJobTitle(int id, [FromBody] UpsertLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveJobTitleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("employees")]
    public async Task<IActionResult> SearchEmployees(
        [FromQuery] string? search,
        [FromQuery] EmployeeStatus? status,
        [FromQuery] int? departmentId,
        [FromQuery] int? jobTitleId,
        [FromQuery] EmployeeAccountType? accountType,
        CancellationToken cancellationToken)
    {
        var result = await humanResources.SearchEmployeesAsync(new EmployeeSearchRequest(search, status, departmentId, jobTitleId, accountType), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("employees/{id:int}")]
    public async Task<IActionResult> GetEmployee(int id, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetEmployeeAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.CreateEmployeeAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("employees/{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.UpdateEmployeeAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("employees/{id:int}/terminate")]
    public async Task<IActionResult> TerminateEmployee(int id, [FromBody] TerminateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.TerminateEmployeeAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("employees/{id:int}/restore")]
    public async Task<IActionResult> RestoreEmployee(int id, CancellationToken cancellationToken)
    {
        var result = await humanResources.RestoreEmployeeAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] int? employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetAttendanceAsync(employeeId, from, to, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance")]
    public async Task<IActionResult> RecordAttendance([FromBody] RecordEmployeeAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.RecordAttendanceAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("leave-requests")]
    public async Task<IActionResult> LeaveRequests([FromQuery] int? employeeId, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetLeaveRequestsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("leave-requests")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateEmployeeLeaveRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.CreateLeaveRequestAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("leave-requests/{id:int}/decision")]
    public async Task<IActionResult> DecideLeaveRequest(int id, [FromBody] DecideEmployeeLeaveRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.DecideLeaveRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("documents")]
    public async Task<IActionResult> Documents([FromQuery] int? employeeId, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetDocumentsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("documents")]
    public async Task<IActionResult> AddDocument([FromBody] AddEmployeeDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.AddDocumentAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("disciplinary-records")]
    public async Task<IActionResult> DisciplinaryRecords([FromQuery] int? employeeId, [FromQuery] EmployeeDisciplinaryRecordType? type, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetDisciplinaryRecordsAsync(employeeId, type, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("disciplinary-records")]
    public async Task<IActionResult> CreateDisciplinaryRecord([FromBody] CreateEmployeeDisciplinaryRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.CreateDisciplinaryRecordAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("disciplinary-records/{id:int}/decision")]
    public async Task<IActionResult> DecideDisciplinaryRecord(int id, [FromBody] DecideHumanResourceItemRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.DecideDisciplinaryRecordAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("leave-balances")]
    public async Task<IActionResult> LeaveBalances([FromQuery] int? employeeId, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetLeaveBalancesAsync(employeeId, year, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("leave-balances")]
    public async Task<IActionResult> CreateLeaveBalance([FromBody] SaveEmployeeLeaveBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveLeaveBalanceAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("leave-balances/{id:int}")]
    public async Task<IActionResult> UpdateLeaveBalance(int id, [FromBody] SaveEmployeeLeaveBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveLeaveBalanceAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("evaluations")]
    public async Task<IActionResult> Evaluations([FromQuery] int? employeeId, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetEvaluationsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("evaluations")]
    public async Task<IActionResult> CreateEvaluation([FromBody] SaveEmployeeEvaluationRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveEvaluationAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("evaluations/{id:int}")]
    public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] SaveEmployeeEvaluationRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveEvaluationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("cards")]
    public async Task<IActionResult> Cards([FromQuery] int? employeeId, [FromQuery] string? cardType, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetCardIssuesAsync(employeeId, cardType, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("cards")]
    public async Task<IActionResult> IssueCard([FromBody] IssueEmployeeCardRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.IssueCardAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("letters")]
    public async Task<IActionResult> Letters([FromQuery] int? employeeId, [FromQuery] HumanResourceRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetLetterRequestsAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("letters")]
    public async Task<IActionResult> CreateLetter([FromBody] SaveEmployeeLetterRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveLetterRequestAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("letters/{id:int}")]
    public async Task<IActionResult> UpdateLetter(int id, [FromBody] SaveEmployeeLetterRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveLetterRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("letters/{id:int}/decision")]
    public async Task<IActionResult> DecideLetter(int id, [FromBody] DecideHumanResourceItemRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.DecideLetterRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("payroll")]
    public async Task<IActionResult> Payroll([FromQuery] DateTime? payrollMonth, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetPayrollRecordsAsync(payrollMonth, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("payroll/preview")]
    public async Task<IActionResult> GeneratePayrollPreview([FromBody] GeneratePayrollPreviewRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.GeneratePayrollPreviewAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("attendance-policies")]
    public async Task<IActionResult> AttendancePolicies(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetAttendancePoliciesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance-policies")]
    public async Task<IActionResult> CreateAttendancePolicy([FromBody] SaveAttendancePolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveAttendancePolicyAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("attendance-policies/{id:int}")]
    public async Task<IActionResult> UpdateAttendancePolicy(int id, [FromBody] SaveAttendancePolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveAttendancePolicyAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("attendance-locations")]
    public async Task<IActionResult> AttendanceLocations(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetAttendanceLocationsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance-locations")]
    public async Task<IActionResult> CreateAttendanceLocation([FromBody] SaveAttendanceLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveAttendanceLocationAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("official-vacations")]
    public async Task<IActionResult> OfficialVacations([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetOfficialVacationsAsync(year, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("official-vacations")]
    public async Task<IActionResult> CreateOfficialVacation([FromBody] SaveOfficialVacationRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveOfficialVacationAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("attendance-excuses")]
    public async Task<IActionResult> AttendanceExcuses([FromQuery] int? employeeId, [FromQuery] HumanResourceRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetAttendanceExcusesAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance-excuses")]
    public async Task<IActionResult> CreateAttendanceExcuse([FromBody] CreateAttendanceExcuseRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.CreateAttendanceExcuseAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance-excuses/{id:int}/decision")]
    public async Task<IActionResult> DecideAttendanceExcuse(int id, [FromBody] DecideHumanResourceItemRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.DecideAttendanceExcuseAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("presence")]
    public async Task<IActionResult> Presence([FromQuery] DateTime? workDate, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetCurrentPresenceAsync(workDate, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("safety/categories")]
    public async Task<IActionResult> SafetyCategories(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetSafetyCategoriesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("safety/categories")]
    public async Task<IActionResult> SaveSafetyCategory([FromBody] SaveSafetyCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveSafetyCategoryAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("safety/procedures")]
    public async Task<IActionResult> SafetyProcedures(CancellationToken cancellationToken)
    {
        var result = await humanResources.GetSafetyProceduresAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("safety/procedures")]
    public async Task<IActionResult> SaveSafetyProcedure([FromBody] SaveSafetyProcedureRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveSafetyProcedureAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("safety/inspections")]
    public async Task<IActionResult> SafetyInspections([FromQuery] SafetyRecordStatus? status, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetSafetyInspectionsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("safety/inspections")]
    public async Task<IActionResult> SaveSafetyInspection([FromBody] SaveSafetyInspectionRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveSafetyInspectionAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("recruitment")]
    public async Task<IActionResult> Recruitment([FromQuery] RecruitmentRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetRecruitmentRequestsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recruitment")]
    public async Task<IActionResult> CreateRecruitment([FromBody] SaveRecruitmentRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveRecruitmentRequestAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("recruitment/{id:int}")]
    public async Task<IActionResult> UpdateRecruitment(int id, [FromBody] SaveRecruitmentRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.SaveRecruitmentRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("recruitment/{id:int}/status")]
    public async Task<IActionResult> UpdateRecruitmentStatus(int id, [FromBody] UpdateRecruitmentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.UpdateRecruitmentStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("administrative-requests")]
    public async Task<IActionResult> AdministrativeRequests([FromQuery] int? employeeId, [FromQuery] HumanResourceRequestStatus? status, CancellationToken cancellationToken)
    {
        var result = await humanResources.GetAdministrativeRequestsAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("administrative-requests")]
    public async Task<IActionResult> CreateAdministrativeRequest([FromBody] CreateEmployeeAdministrativeRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.CreateAdministrativeRequestAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("administrative-requests/{id:int}/decision")]
    public async Task<IActionResult> DecideAdministrativeRequest(int id, [FromBody] DecideHumanResourceItemRequest request, CancellationToken cancellationToken)
    {
        var result = await humanResources.DecideAdministrativeRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
