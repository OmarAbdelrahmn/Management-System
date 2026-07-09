using Domain.Entities;

namespace Application.Contracts.ProgramsProjects;

public record ProgramsProjectsDashboardResponse(
    int ProjectsCount,
    int ActiveProjectsCount,
    int CompletedProjectsCount,
    int SuppliersCount,
    int PendingIdeasCount,
    int PendingApprovalsCount,
    decimal TotalBudget,
    decimal TotalIncome,
    decimal TotalExpenses);

public record ProgramProjectResponse(
    int Id,
    string ProjectCode,
    string Name,
    string ProjectType,
    string? Description,
    string? ManagerName,
    DateTime? StartsAt,
    DateTime? EndsAt,
    string Status,
    decimal Budget,
    decimal TargetBeneficiaries,
    decimal IncomeTotal,
    decimal ExpenseTotal,
    decimal Balance,
    int TasksCount,
    int CompletedTasksCount,
    bool IsPublished,
    DateTime? PublishedAt,
    string? RegistrationFormJson,
    string? SpecialProgramCategory,
    string? Notes,
    DateTime CreatedAt);

public record ProgramProjectSearchRequest(
    string? Search,
    ProgramProjectStatus? Status,
    string? ProjectType);

public record SaveProgramProjectRequest(
    string Name,
    string? ProjectCode,
    string ProjectType,
    string? Description,
    string? ManagerName,
    DateTime? StartsAt,
    DateTime? EndsAt,
    ProgramProjectStatus Status,
    decimal Budget,
    decimal TargetBeneficiaries,
    string? Notes);

public record UpdateProgramProjectStatusRequest(
    ProgramProjectStatus Status,
    string? Notes);

public record PublishProgramProjectRequest(
    bool IsPublished,
    DateTime? PublishedAt,
    string? Notes);

public record SaveProgramRegistrationFormRequest(
    string? SpecialProgramCategory,
    string RegistrationFormJson);

public record ProgramProjectTaskResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string Title,
    string? OwnerName,
    DateTime? DueDate,
    string Status,
    decimal ProgressPercent,
    string? Notes);

public record SaveProgramProjectTaskRequest(
    int ProgramProjectId,
    string Title,
    string? OwnerName,
    DateTime? DueDate,
    ProgramProjectTaskStatus Status,
    decimal ProgressPercent,
    string? Notes);

public record ProgramProjectMilestoneResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string Title,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal ProgressPercent,
    string? Notes);

public record SaveProgramProjectMilestoneRequest(
    int ProgramProjectId,
    string Title,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal ProgressPercent,
    string? Notes);

public record ProgramProjectContractResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    int? ProgramSupplierId,
    string? SupplierName,
    string ContractNumber,
    string Title,
    decimal Amount,
    DateTime SignedAt,
    DateTime? EndsAt,
    string? Notes);

public record SaveProgramProjectContractRequest(
    int ProgramProjectId,
    int? ProgramSupplierId,
    string? ContractNumber,
    string Title,
    decimal Amount,
    DateTime SignedAt,
    DateTime? EndsAt,
    string? Notes);

public record ProgramProjectFinanceEntryResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string EntryType,
    DateTime EntryDate,
    decimal Amount,
    string SourceOrPayee,
    string? ReferenceNumber,
    string? Notes);

public record AddProgramProjectFinanceEntryRequest(
    int ProgramProjectId,
    ProgramProjectFinanceEntryType EntryType,
    DateTime? EntryDate,
    decimal Amount,
    string SourceOrPayee,
    string? ReferenceNumber,
    string? Notes);

public record ProgramProjectAssignmentResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string AssignmentType,
    string DisplayName,
    string? ExternalReference,
    DateTime AssignedAt,
    string? Notes);

public record AddProgramProjectAssignmentRequest(
    int ProgramProjectId,
    ProgramProjectAssignmentType AssignmentType,
    string DisplayName,
    string? ExternalReference,
    DateTime? AssignedAt,
    string? Notes);

public record ProgramProjectReportResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string ReportType,
    DateTime ReportDate,
    string Summary,
    string? FilePath);

public record ProgramProjectActivityResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string Type,
    string Title,
    string? Note,
    string? FromStatus,
    string? ToStatus,
    decimal? Amount,
    string? Reference,
    DateTime OccurredAt);

public record AddProgramProjectReportRequest(
    int ProgramProjectId,
    string ReportType,
    DateTime? ReportDate,
    string Summary,
    string? FilePath);

public record ProgramSupplierResponse(
    int Id,
    string Name,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? City,
    string Status,
    string? Notes);

public record SaveProgramSupplierRequest(
    string Name,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? City,
    ProgramSupplierStatus Status,
    string? Notes);

public record ProgramIdeaResponse(
    int Id,
    string Title,
    string? OwnerName,
    string Description,
    string? MarketingNotes,
    decimal EstimatedBudget,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt,
    DateTime CreatedAt);

public record SaveProgramIdeaRequest(
    string Title,
    string? OwnerName,
    string Description,
    string? MarketingNotes,
    decimal EstimatedBudget,
    ProgramIdeaStatus Status);

public record UpdateProgramIdeaStatusRequest(
    ProgramIdeaStatus Status,
    string? DecisionNotes);

public record ProgramApprovalResponse(
    int Id,
    int? ProgramIdeaId,
    string? ProgramIdeaTitle,
    string ApprovalType,
    string Title,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt);

public record SaveProgramApprovalRequest(
    int? ProgramIdeaId,
    string ApprovalType,
    string Title);

public record DecideProgramApprovalRequest(
    ProgramApprovalStatus Status,
    string? DecisionNotes);

public record ProgramRegistrationResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string ParticipantName,
    string? Mobile,
    string? Email,
    string? ExternalReference,
    string Status,
    DateTime RegisteredAt,
    string? DecisionNotes,
    DateTime? DecidedAt,
    string? Notes);

public record SaveProgramRegistrationRequest(
    int ProgramProjectId,
    string ParticipantName,
    string? Mobile,
    string? Email,
    string? ExternalReference,
    DateTime? RegisteredAt,
    string? Notes);

public record DecideProgramRegistrationRequest(
    ProgramRegistrationStatus Status,
    string? DecisionNotes);

public record ProgramSessionResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string Title,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    string? Notes,
    int PresentCount,
    int AbsentCount,
    int ExcusedCount);

public record SaveProgramSessionRequest(
    int ProgramProjectId,
    string Title,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    string? Notes);

public record ProgramSessionAttendanceResponse(
    int Id,
    int ProgramSessionId,
    string SessionTitle,
    string ProjectName,
    string ParticipantName,
    string? ExternalReference,
    string Status,
    string? Notes);

public record SaveProgramSessionAttendanceRequest(
    int ProgramSessionId,
    string ParticipantName,
    string? ExternalReference,
    ProgramAttendanceStatus Status,
    string? Notes);

public record ProgramSurveyResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    string Title,
    string QuestionsJson,
    string Status,
    string? Notes,
    int SubmissionsCount);

public record SaveProgramSurveyRequest(
    int ProgramProjectId,
    string Title,
    string QuestionsJson,
    ProgramSurveyStatus Status,
    string? Notes);

public record ProgramSurveySubmissionResponse(
    int Id,
    int ProgramSurveyId,
    string SurveyTitle,
    string RespondentName,
    string AnswersJson,
    DateTime SubmittedAt);

public record AddProgramSurveySubmissionRequest(
    int ProgramSurveyId,
    string RespondentName,
    string AnswersJson,
    DateTime? SubmittedAt);

public record ProgramCertificateTemplateResponse(
    int Id,
    int? ProgramProjectId,
    string? ProjectName,
    string Name,
    string BodyTemplate,
    bool IsActive);

public record SaveProgramCertificateTemplateRequest(
    int? ProgramProjectId,
    string Name,
    string BodyTemplate,
    bool IsActive);

public record ProgramCertificateIssueResponse(
    int Id,
    int ProgramProjectId,
    string ProjectName,
    int? ProgramCertificateTemplateId,
    string? TemplateName,
    string CertificateNumber,
    string RecipientName,
    DateTime IssuedAt,
    string Status,
    string? Notes);

public record IssueProgramCertificateRequest(
    int ProgramProjectId,
    int? ProgramCertificateTemplateId,
    string? CertificateNumber,
    string RecipientName,
    DateTime? IssuedAt,
    string? Notes);

public record ProgramQualificationCaseResponse(
    int Id,
    int? ProgramProjectId,
    string? ProjectName,
    string BeneficiaryName,
    string NeedSummary,
    string? ManagementOpinion,
    string Status,
    decimal ApprovedAmount,
    int InstallmentCount,
    decimal PaidAmount,
    decimal RemainingAmount,
    string? Notes);

public record SaveProgramQualificationCaseRequest(
    int? ProgramProjectId,
    string BeneficiaryName,
    string NeedSummary,
    string? ManagementOpinion,
    ProgramQualificationCaseStatus Status,
    decimal ApprovedAmount,
    int InstallmentCount,
    string? Notes);

public record UpdateProgramQualificationCaseStatusRequest(
    ProgramQualificationCaseStatus Status,
    string? ManagementOpinion,
    string? Notes);

public record ProgramQualificationInstallmentResponse(
    int Id,
    int ProgramQualificationCaseId,
    string BeneficiaryName,
    DateTime DueDate,
    decimal Amount,
    decimal PaidAmount,
    DateTime? PaidAt,
    string Status,
    string? Notes);

public record SaveProgramQualificationInstallmentRequest(
    int ProgramQualificationCaseId,
    DateTime DueDate,
    decimal Amount,
    string? Notes);

public record RecordQualificationInstallmentPaymentRequest(
    decimal PaidAmount,
    DateTime? PaidAt,
    string? Notes);
