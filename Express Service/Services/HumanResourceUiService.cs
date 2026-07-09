using Application.Contracts.HumanResources;
using Application.Service.HumanResources;
using Domain.Entities;

namespace Express_Service.Services;

public class HumanResourceUiService(IHumanResourceService humanResources)
{
    public async Task<HumanResourceDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<EmployeeDepartmentResponse>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetDepartmentsAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<JobTitleResponse>> GetJobTitlesAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetJobTitlesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<EmployeeResponse>> SearchEmployeesAsync(string? search, EmployeeStatus? status, int? departmentId, int? jobTitleId, EmployeeAccountType? accountType = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SearchEmployeesAsync(new EmployeeSearchRequest(search, status, departmentId, jobTitleId, accountType), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<EmployeeResponse?> GetEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetEmployeeAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<(bool Success, string Message, EmployeeResponse? Employee)> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.CreateEmployeeAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة الموظف بنجاح.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message, EmployeeResponse? Employee)> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.UpdateEmployeeAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث بيانات الموظف.", result.Value) : (false, result.Error.Description, null);
    }

    public async Task<(bool Success, string Message)> TerminateEmployeeAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.TerminateEmployeeAsync(id, new TerminateEmployeeRequest(reason), cancellationToken);
        return result.IsSuccess ? (true, "تم إنهاء خدمة الموظف.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SaveDepartmentAsync(int? id, UpsertLookupRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveDepartmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ القسم.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SaveJobTitleAsync(int? id, UpsertLookupRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveJobTitleAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ المسمى الوظيفي.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeAttendanceResponse>> GetAttendanceAsync(int? employeeId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetAttendanceAsync(employeeId, from, to, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> RecordAttendanceAsync(RecordEmployeeAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.RecordAttendanceAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل الحضور.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeLeaveRequestResponse>> GetLeaveRequestsAsync(int? employeeId = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetLeaveRequestsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateLeaveRequestAsync(CreateEmployeeLeaveRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.CreateLeaveRequestAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء طلب الإجازة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideLeaveRequestAsync(int id, bool approved, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.DecideLeaveRequestAsync(id, new DecideEmployeeLeaveRequest(approved, notes), cancellationToken);
        return result.IsSuccess ? (true, approved ? "تم اعتماد الإجازة." : "تم رفض الإجازة.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeDocumentResponse>> GetDocumentsAsync(int? employeeId = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetDocumentsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddDocumentAsync(AddEmployeeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.AddDocumentAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة المستند.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeDisciplinaryRecordResponse>> GetDisciplinaryRecordsAsync(int? employeeId = null, EmployeeDisciplinaryRecordType? type = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetDisciplinaryRecordsAsync(employeeId, type, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateDisciplinaryRecordAsync(CreateEmployeeDisciplinaryRecordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.CreateDisciplinaryRecordAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل الإجراء.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideDisciplinaryRecordAsync(int id, HumanResourceRequestStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.DecideDisciplinaryRecordAsync(id, new DecideHumanResourceItemRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الإجراء.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeLeaveBalanceResponse>> GetLeaveBalancesAsync(int? employeeId = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetLeaveBalancesAsync(employeeId, year, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveLeaveBalanceAsync(int? id, SaveEmployeeLeaveBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveLeaveBalanceAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ رصيد الإجازة.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeEvaluationResponse>> GetEvaluationsAsync(int? employeeId = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetEvaluationsAsync(employeeId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveEvaluationAsync(int? id, SaveEmployeeEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveEvaluationAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ التقييم.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeCardIssueResponse>> GetCardIssuesAsync(int? employeeId = null, string? cardType = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetCardIssuesAsync(employeeId, cardType, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> IssueCardAsync(IssueEmployeeCardRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.IssueCardAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إصدار البطاقة.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeLetterRequestResponse>> GetLetterRequestsAsync(int? employeeId = null, HumanResourceRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetLetterRequestsAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveLetterRequestAsync(int? id, SaveEmployeeLetterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveLetterRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الخطاب.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideLetterRequestAsync(int id, HumanResourceRequestStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.DecideLetterRequestAsync(id, new DecideHumanResourceItemRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الخطاب.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeePayrollRecordResponse>> GetPayrollRecordsAsync(DateTime? payrollMonth = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetPayrollRecordsAsync(payrollMonth, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> GeneratePayrollPreviewAsync(GeneratePayrollPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GeneratePayrollPreviewAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم توليد معاينة الرواتب.") : (false, result.Error.Description);
    }

    public async Task<List<AttendancePolicyResponse>> GetAttendancePoliciesAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetAttendancePoliciesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveAttendancePolicyAsync(int? id, SaveAttendancePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveAttendancePolicyAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ إعدادات الحضور.") : (false, result.Error.Description);
    }

    public async Task<List<AttendanceLocationResponse>> GetAttendanceLocationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetAttendanceLocationsAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveAttendanceLocationAsync(int? id, SaveAttendanceLocationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveAttendanceLocationAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ موقع التسجيل.") : (false, result.Error.Description);
    }

    public async Task<List<OfficialVacationResponse>> GetOfficialVacationsAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetOfficialVacationsAsync(year, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveOfficialVacationAsync(int? id, SaveOfficialVacationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveOfficialVacationAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الإجازة الرسمية.") : (false, result.Error.Description);
    }

    public async Task<List<AttendanceExcuseResponse>> GetAttendanceExcusesAsync(int? employeeId = null, HumanResourceRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetAttendanceExcusesAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateAttendanceExcuseAsync(CreateAttendanceExcuseRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.CreateAttendanceExcuseAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل الإذن.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideAttendanceExcuseAsync(int id, HumanResourceRequestStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.DecideAttendanceExcuseAsync(id, new DecideHumanResourceItemRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الإذن.") : (false, result.Error.Description);
    }

    public async Task<List<CurrentPresenceResponse>> GetCurrentPresenceAsync(DateTime? workDate = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetCurrentPresenceAsync(workDate, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<SafetyCategoryResponse>> GetSafetyCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetSafetyCategoriesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSafetyCategoryAsync(int? id, SaveSafetyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveSafetyCategoryAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ تصنيف الخطر.") : (false, result.Error.Description);
    }

    public async Task<List<SafetyProcedureResponse>> GetSafetyProceduresAsync(CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetSafetyProceduresAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSafetyProcedureAsync(int? id, SaveSafetyProcedureRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveSafetyProcedureAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ إجراء السلامة.") : (false, result.Error.Description);
    }

    public async Task<List<SafetyInspectionResponse>> GetSafetyInspectionsAsync(SafetyRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetSafetyInspectionsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSafetyInspectionAsync(int? id, SaveSafetyInspectionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveSafetyInspectionAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ نموذج السلامة.") : (false, result.Error.Description);
    }

    public async Task<List<RecruitmentRequestResponse>> GetRecruitmentRequestsAsync(RecruitmentRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetRecruitmentRequestsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveRecruitmentRequestAsync(int? id, SaveRecruitmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.SaveRecruitmentRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ طلب التوظيف.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateRecruitmentStatusAsync(int id, UpdateRecruitmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.UpdateRecruitmentStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة طلب التوظيف.") : (false, result.Error.Description);
    }

    public async Task<List<EmployeeAdministrativeRequestResponse>> GetAdministrativeRequestsAsync(int? employeeId = null, HumanResourceRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.GetAdministrativeRequestsAsync(employeeId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateAdministrativeRequestAsync(CreateEmployeeAdministrativeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.CreateAdministrativeRequestAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء الطلب الإداري.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideAdministrativeRequestAsync(int id, HumanResourceRequestStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await humanResources.DecideAdministrativeRequestAsync(id, new DecideHumanResourceItemRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة الطلب الإداري.") : (false, result.Error.Description);
    }
}
