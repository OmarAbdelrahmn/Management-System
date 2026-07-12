using Application.Contracts.ProgramsProjects;
using Application.Service.ProgramsProjects;
using Domain.Entities;

namespace Express_Service.Services;

public class ProgramsProjectsUiService(IProgramsProjectsService programsProjects)
{
    public async Task<ProgramsProjectsDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<ProgramProjectResponse>> SearchProjectsAsync(string? search = null, ProgramProjectStatus? status = null, string? projectType = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SearchProjectsAsync(new ProgramProjectSearchRequest(search, status, projectType), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveProjectAsync(int? id, SaveProgramProjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ المشروع/البرنامج.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateProjectStatusAsync(int id, ProgramProjectStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateProjectStatusAsync(id, new UpdateProgramProjectStatusRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة المشروع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> PublishProjectAsync(int id, bool isPublished, string? notes = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.PublishProjectAsync(id, new PublishProgramProjectRequest(isPublished, DateTime.UtcNow.AddHours(3), notes), cancellationToken);
        return result.IsSuccess ? (true, isPublished ? "تم نشر البرنامج." : "تم إلغاء نشر البرنامج.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SaveRegistrationFormAsync(int id, SaveProgramRegistrationFormRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveRegistrationFormAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ نموذج التسجيل.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectTaskResponse>> GetTasksAsync(int? projectId = null, ProgramProjectTaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetTasksAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveTaskAsync(int? id, SaveProgramProjectTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ مهمة المشروع.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectMilestoneResponse>> GetMilestonesAsync(int? projectId = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetMilestonesAsync(projectId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveMilestoneAsync(int? id, SaveProgramProjectMilestoneRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveMilestoneAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ نشاط المشروع.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectContractResponse>> GetContractsAsync(int? projectId = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetContractsAsync(projectId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveContractAsync(int? id, SaveProgramProjectContractRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveContractAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ العقد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RecordContractPaymentAsync(int contractId, RecordProgramContractPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.RecordContractPaymentAsync(contractId, request, cancellationToken);
        return result.IsSuccess ? (true, $"تم تسجيل سداد العقد. المتبقي {result.Value.RemainingAmount:N2}.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectFinanceEntryResponse>> GetFinanceEntriesAsync(int? projectId = null, ProgramProjectFinanceEntryType? entryType = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetFinanceEntriesAsync(projectId, entryType, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddFinanceEntryAsync(AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.AddFinanceEntryAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل الحركة المالية.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateFinanceEntryAsync(int id, AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateFinanceEntryAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث الحركة المالية.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectAssignmentResponse>> GetAssignmentsAsync(int? projectId = null, ProgramProjectAssignmentType? assignmentType = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetAssignmentsAsync(projectId, assignmentType, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddAssignmentAsync(AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.AddAssignmentAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إلحاق السجل بالمشروع.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateAssignmentAsync(int id, AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateAssignmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث الإلحاق.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectReportResponse>> GetReportsAsync(int? projectId = null, string? reportType = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetReportsAsync(projectId, reportType, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddReportAsync(AddProgramProjectReportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.AddReportAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ التقرير.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateReportAsync(int id, AddProgramProjectReportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateReportAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث التقرير.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CloseProjectAsync(int id, CloseProgramProjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.CloseProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم إغلاق المشروع/البرنامج بالتقرير الختامي.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramProjectActivityResponse>> GetProjectActivitiesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetProjectActivitiesAsync(projectId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<ProgramSupplierResponse>> GetSuppliersAsync(string? search = null, ProgramSupplierStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetSuppliersAsync(search, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSupplierAsync(int? id, SaveProgramSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveSupplierAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ المورد.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramSupplierProposalResponse>> GetSupplierProposalsAsync(int? projectId = null, int? supplierId = null, ProgramSupplierProposalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetSupplierProposalsAsync(projectId, supplierId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSupplierProposalAsync(int? id, SaveProgramSupplierProposalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveSupplierProposalAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ عرض المورد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideSupplierProposalAsync(int id, ProgramSupplierProposalStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.DecideSupplierProposalAsync(id, new DecideProgramSupplierProposalRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث قرار عرض المورد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> ConvertSupplierProposalToContractAsync(int id, ConvertProgramSupplierProposalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.ConvertSupplierProposalToContractAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحويل عرض المورد إلى عقد.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramIdeaResponse>> GetIdeasAsync(ProgramIdeaStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetIdeasAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveIdeaAsync(int? id, SaveProgramIdeaRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveIdeaAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ مقترح البرنامج.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateIdeaStatusAsync(int id, ProgramIdeaStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateIdeaStatusAsync(id, new UpdateProgramIdeaStatusRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة المقترح.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> ConvertIdeaToProjectAsync(int id, ConvertProgramIdeaToProjectRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.ConvertIdeaToProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, $"تم تحويل المقترح إلى مشروع/برنامج برقم {result.Value.ProjectCode}.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramApprovalResponse>> GetApprovalsAsync(ProgramApprovalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetApprovalsAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveApprovalAsync(int? id, SaveProgramApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveApprovalAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideApprovalAsync(int id, ProgramApprovalStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.DecideApprovalAsync(id, new DecideProgramApprovalRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث قرار الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramRegistrationResponse>> GetRegistrationsAsync(int? projectId = null, ProgramRegistrationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetRegistrationsAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveRegistrationAsync(int? id, SaveProgramRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveRegistrationAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ تسجيل البرنامج.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideRegistrationAsync(int id, ProgramRegistrationStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.DecideRegistrationAsync(id, new DecideProgramRegistrationRequest(status, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة التسجيل.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramSessionResponse>> GetSessionsAsync(int? projectId = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetSessionsAsync(projectId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSessionAsync(int? id, SaveProgramSessionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveSessionAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ موعد البرنامج.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramSessionAttendanceResponse>> GetAttendanceAsync(int? sessionId = null, ProgramAttendanceStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetAttendanceAsync(sessionId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveAttendanceAsync(int? id, SaveProgramSessionAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveAttendanceAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ حضور البرنامج.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramSurveyResponse>> GetSurveysAsync(int? projectId = null, ProgramSurveyStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetSurveysAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSurveyAsync(int? id, SaveProgramSurveyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveSurveyAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ استبيان البرنامج.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramSurveySubmissionResponse>> GetSurveySubmissionsAsync(int? surveyId = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetSurveySubmissionsAsync(surveyId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddSurveySubmissionAsync(AddProgramSurveySubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.AddSurveySubmissionAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ إجابة الاستبيان.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramCertificateTemplateResponse>> GetCertificateTemplatesAsync(int? projectId = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetCertificateTemplatesAsync(projectId, isActive, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveCertificateTemplateAsync(int? id, SaveProgramCertificateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveCertificateTemplateAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ قالب الشهادة.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramCertificateIssueResponse>> GetCertificateIssuesAsync(int? projectId = null, ProgramCertificateIssueStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetCertificateIssuesAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> IssueCertificateAsync(IssueProgramCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.IssueCertificateAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إصدار الشهادة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> IssueCertificateFromRegistrationAsync(int registrationId, IssueProgramCertificateFromRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.IssueCertificateFromRegistrationAsync(registrationId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم إصدار شهادة حضور التسجيل.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelCertificateIssueAsync(int id, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.CancelCertificateIssueAsync(id, new CancelProgramCertificateIssueRequest(notes), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء الشهادة.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramQualificationCaseResponse>> GetQualificationCasesAsync(ProgramQualificationCaseStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetQualificationCasesAsync(status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveQualificationCaseAsync(int? id, SaveProgramQualificationCaseRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveQualificationCaseAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ مشروع التأهيل.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateQualificationCaseStatusAsync(int id, ProgramQualificationCaseStatus status, string? opinion, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.UpdateQualificationCaseStatusAsync(id, new UpdateProgramQualificationCaseStatusRequest(status, opinion, notes), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث حالة مشروع التأهيل.") : (false, result.Error.Description);
    }

    public async Task<List<ProgramQualificationInstallmentResponse>> GetQualificationInstallmentsAsync(int? caseId = null, ProgramQualificationInstallmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GetQualificationInstallmentsAsync(caseId, status, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> GenerateQualificationInstallmentsAsync(int caseId, GenerateQualificationInstallmentsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.GenerateQualificationInstallmentsAsync(caseId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء جدول أقساط التأهيل.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> SaveQualificationInstallmentAsync(int? id, SaveProgramQualificationInstallmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.SaveQualificationInstallmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ قسط التأهيل.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RecordQualificationInstallmentPaymentAsync(int id, RecordQualificationInstallmentPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await programsProjects.RecordQualificationInstallmentPaymentAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل سداد القسط.") : (false, result.Error.Description);
    }
}
