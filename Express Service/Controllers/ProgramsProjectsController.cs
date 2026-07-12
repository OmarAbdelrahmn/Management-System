using Application.Contracts.ProgramsProjects;
using Application.Service.ProgramsProjects;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class ProgramsProjectsController(IProgramsProjectsService programsProjects) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("projects")]
    public async Task<IActionResult> Projects([FromQuery] string? search, [FromQuery] ProgramProjectStatus? status, [FromQuery] string? projectType, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SearchProjectsAsync(new ProgramProjectSearchRequest(search, status, projectType), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("projects/{id:int}")]
    public async Task<IActionResult> Project(int id, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetProjectAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("projects")]
    public async Task<IActionResult> CreateProject([FromBody] SaveProgramProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveProjectAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("projects/{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] SaveProgramProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("projects/{id:int}/status")]
    public async Task<IActionResult> UpdateProjectStatus(int id, [FromBody] UpdateProgramProjectStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateProjectStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("projects/{id:int}/publish")]
    public async Task<IActionResult> PublishProject(int id, [FromBody] PublishProgramProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.PublishProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("projects/{id:int}/registration-form")]
    public async Task<IActionResult> SaveRegistrationForm(int id, [FromBody] SaveProgramRegistrationFormRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveRegistrationFormAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks([FromQuery] int? projectId, [FromQuery] ProgramProjectTaskStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetTasksAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> SaveTask([FromBody] SaveProgramProjectTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveTaskAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] SaveProgramProjectTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("milestones")]
    public async Task<IActionResult> Milestones([FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetMilestonesAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("milestones")]
    public async Task<IActionResult> SaveMilestone([FromBody] SaveProgramProjectMilestoneRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveMilestoneAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("milestones/{id:int}")]
    public async Task<IActionResult> UpdateMilestone(int id, [FromBody] SaveProgramProjectMilestoneRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveMilestoneAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> Contracts([FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetContractsAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("contracts")]
    public async Task<IActionResult> SaveContract([FromBody] SaveProgramProjectContractRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveContractAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("contracts/{id:int}")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] SaveProgramProjectContractRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveContractAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("contracts/{id:int}/payments")]
    public async Task<IActionResult> RecordContractPayment(int id, [FromBody] RecordProgramContractPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.RecordContractPaymentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("finance")]
    public async Task<IActionResult> Finance([FromQuery] int? projectId, [FromQuery] ProgramProjectFinanceEntryType? entryType, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetFinanceEntriesAsync(projectId, entryType, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("finance")]
    public async Task<IActionResult> AddFinance([FromBody] AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.AddFinanceEntryAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("finance/{id:int}")]
    public async Task<IActionResult> UpdateFinance(int id, [FromBody] AddProgramProjectFinanceEntryRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateFinanceEntryAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> Assignments([FromQuery] int? projectId, [FromQuery] ProgramProjectAssignmentType? assignmentType, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetAssignmentsAsync(projectId, assignmentType, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("assignments")]
    public async Task<IActionResult> AddAssignment([FromBody] AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.AddAssignmentAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("assignments/{id:int}")]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] AddProgramProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateAssignmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("reports")]
    public async Task<IActionResult> Reports([FromQuery] int? projectId, [FromQuery] string? reportType, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetReportsAsync(projectId, reportType, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("reports")]
    public async Task<IActionResult> AddReport([FromBody] AddProgramProjectReportRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.AddReportAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("reports/{id:int}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] AddProgramProjectReportRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateReportAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("projects/{id:int}/close")]
    public async Task<IActionResult> CloseProject(int id, [FromBody] CloseProgramProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.CloseProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("projects/{projectId:int}/activities")]
    public async Task<IActionResult> ProjectActivities(int projectId, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetProjectActivitiesAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers([FromQuery] string? search, [FromQuery] ProgramSupplierStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetSuppliersAsync(search, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> SaveSupplier([FromBody] SaveProgramSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSupplierAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("suppliers/{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SaveProgramSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSupplierAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("supplier-proposals")]
    public async Task<IActionResult> SupplierProposals(
        [FromQuery] int? projectId,
        [FromQuery] int? supplierId,
        [FromQuery] ProgramSupplierProposalStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetSupplierProposalsAsync(projectId, supplierId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("supplier-proposals")]
    public async Task<IActionResult> SaveSupplierProposal([FromBody] SaveProgramSupplierProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSupplierProposalAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("supplier-proposals/{id:int}")]
    public async Task<IActionResult> UpdateSupplierProposal(int id, [FromBody] SaveProgramSupplierProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSupplierProposalAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("supplier-proposals/{id:int}/decision")]
    public async Task<IActionResult> DecideSupplierProposal(int id, [FromBody] DecideProgramSupplierProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.DecideSupplierProposalAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("supplier-proposals/{id:int}/convert-contract")]
    public async Task<IActionResult> ConvertSupplierProposal(int id, [FromBody] ConvertProgramSupplierProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.ConvertSupplierProposalToContractAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("ideas")]
    public async Task<IActionResult> Ideas([FromQuery] ProgramIdeaStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetIdeasAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("ideas")]
    public async Task<IActionResult> SaveIdea([FromBody] SaveProgramIdeaRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveIdeaAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("ideas/{id:int}")]
    public async Task<IActionResult> UpdateIdea(int id, [FromBody] SaveProgramIdeaRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveIdeaAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("ideas/{id:int}/status")]
    public async Task<IActionResult> UpdateIdeaStatus(int id, [FromBody] UpdateProgramIdeaStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateIdeaStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("ideas/{id:int}/convert-project")]
    public async Task<IActionResult> ConvertIdeaToProject(int id, [FromBody] ConvertProgramIdeaToProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.ConvertIdeaToProjectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("approvals")]
    public async Task<IActionResult> Approvals([FromQuery] ProgramApprovalStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetApprovalsAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("approvals")]
    public async Task<IActionResult> SaveApproval([FromBody] SaveProgramApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveApprovalAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("approvals/{id:int}")]
    public async Task<IActionResult> UpdateApproval(int id, [FromBody] SaveProgramApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveApprovalAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("approvals/{id:int}/decision")]
    public async Task<IActionResult> DecideApproval(int id, [FromBody] DecideProgramApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.DecideApprovalAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("registrations")]
    public async Task<IActionResult> Registrations([FromQuery] int? projectId, [FromQuery] ProgramRegistrationStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetRegistrationsAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("registrations")]
    public async Task<IActionResult> SaveRegistration([FromBody] SaveProgramRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveRegistrationAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("registrations/{id:int}")]
    public async Task<IActionResult> UpdateRegistration(int id, [FromBody] SaveProgramRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveRegistrationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("registrations/{id:int}/decision")]
    public async Task<IActionResult> DecideRegistration(int id, [FromBody] DecideProgramRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.DecideRegistrationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions([FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetSessionsAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> SaveSession([FromBody] SaveProgramSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSessionAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("sessions/{id:int}")]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] SaveProgramSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSessionAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] int? sessionId, [FromQuery] ProgramAttendanceStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetAttendanceAsync(sessionId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("attendance")]
    public async Task<IActionResult> SaveAttendance([FromBody] SaveProgramSessionAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveAttendanceAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("attendance/{id:int}")]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] SaveProgramSessionAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveAttendanceAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("surveys")]
    public async Task<IActionResult> Surveys([FromQuery] int? projectId, [FromQuery] ProgramSurveyStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetSurveysAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("surveys")]
    public async Task<IActionResult> SaveSurvey([FromBody] SaveProgramSurveyRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSurveyAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("surveys/{id:int}")]
    public async Task<IActionResult> UpdateSurvey(int id, [FromBody] SaveProgramSurveyRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveSurveyAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("survey-submissions")]
    public async Task<IActionResult> SurveySubmissions([FromQuery] int? surveyId, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetSurveySubmissionsAsync(surveyId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("survey-submissions")]
    public async Task<IActionResult> AddSurveySubmission([FromBody] AddProgramSurveySubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.AddSurveySubmissionAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("certificate-templates")]
    public async Task<IActionResult> CertificateTemplates([FromQuery] int? projectId, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetCertificateTemplatesAsync(projectId, isActive, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("certificate-templates")]
    public async Task<IActionResult> SaveCertificateTemplate([FromBody] SaveProgramCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveCertificateTemplateAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("certificate-templates/{id:int}")]
    public async Task<IActionResult> UpdateCertificateTemplate(int id, [FromBody] SaveProgramCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveCertificateTemplateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("certificate-issues")]
    public async Task<IActionResult> CertificateIssues([FromQuery] int? projectId, [FromQuery] ProgramCertificateIssueStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetCertificateIssuesAsync(projectId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("certificate-issues")]
    public async Task<IActionResult> IssueCertificate([FromBody] IssueProgramCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.IssueCertificateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("registrations/{id:int}/certificate")]
    public async Task<IActionResult> IssueCertificateFromRegistration(int id, [FromBody] IssueProgramCertificateFromRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.IssueCertificateFromRegistrationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("certificate-issues/{id:int}/cancel")]
    public async Task<IActionResult> CancelCertificateIssue(int id, [FromBody] CancelProgramCertificateIssueRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.CancelCertificateIssueAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("qualification-cases")]
    public async Task<IActionResult> QualificationCases([FromQuery] ProgramQualificationCaseStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetQualificationCasesAsync(status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("qualification-cases")]
    public async Task<IActionResult> SaveQualificationCase([FromBody] SaveProgramQualificationCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveQualificationCaseAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("qualification-cases/{id:int}")]
    public async Task<IActionResult> UpdateQualificationCase(int id, [FromBody] SaveProgramQualificationCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveQualificationCaseAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("qualification-cases/{id:int}/status")]
    public async Task<IActionResult> UpdateQualificationCaseStatus(int id, [FromBody] UpdateProgramQualificationCaseStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.UpdateQualificationCaseStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("qualification-installments")]
    public async Task<IActionResult> QualificationInstallments([FromQuery] int? caseId, [FromQuery] ProgramQualificationInstallmentStatus? status, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GetQualificationInstallmentsAsync(caseId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("qualification-cases/{id:int}/installments/generate")]
    public async Task<IActionResult> GenerateQualificationInstallments(int id, [FromBody] GenerateQualificationInstallmentsRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.GenerateQualificationInstallmentsAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("qualification-installments")]
    public async Task<IActionResult> SaveQualificationInstallment([FromBody] SaveProgramQualificationInstallmentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveQualificationInstallmentAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("qualification-installments/{id:int}")]
    public async Task<IActionResult> UpdateQualificationInstallment(int id, [FromBody] SaveProgramQualificationInstallmentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.SaveQualificationInstallmentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("qualification-installments/{id:int}/payment")]
    public async Task<IActionResult> RecordQualificationPayment(int id, [FromBody] RecordQualificationInstallmentPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await programsProjects.RecordQualificationInstallmentPaymentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
