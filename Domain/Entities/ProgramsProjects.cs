using Domain.Auditing;

namespace Domain.Entities;

public enum ProgramProjectStatus
{
    Draft = 0,
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5,
    Deleted = 6
}

public enum ProgramProjectTaskStatus
{
    New = 0,
    Running = 1,
    Blocked = 2,
    Completed = 3,
    Finished = 4,
    Cancelled = 5
}

public enum ProgramProjectFinanceEntryType
{
    Income = 0,
    Expense = 1,
    BalanceTransfer = 2,
    Custody = 3
}

public enum ProgramProjectAssignmentType
{
    Beneficiary = 0,
    Entity = 1,
    TeamMember = 2
}

public enum ProgramProjectActivityType
{
    Created = 0,
    Updated = 1,
    StatusChanged = 2,
    Published = 3,
    RegistrationFormSaved = 4,
    TaskSaved = 5,
    MilestoneSaved = 6,
    ContractSaved = 7,
    FinanceEntryAdded = 8,
    AssignmentAdded = 9,
    ReportAdded = 10,
    RegistrationSaved = 11,
    RegistrationDecided = 12,
    SessionSaved = 13,
    AttendanceSaved = 14,
    SurveySaved = 15,
    CertificateIssued = 16,
    QualificationStatusChanged = 17,
    SupplierProposalSaved = 18,
    SupplierProposalDecided = 19,
    SupplierProposalConverted = 20,
    CertificateCancelled = 21
}

public enum ProgramIdeaStatus
{
    Pending = 0,
    Marketing = 1,
    Approved = 2,
    Note = 3,
    Rejected = 4,
    Cancelled = 5,
    Completed = 6,
    Archived = 7
}

public enum ProgramApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}

public enum ProgramSupplierStatus
{
    Active = 0,
    Inactive = 1,
    Archived = 2
}

public enum ProgramSupplierProposalStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Converted = 5,
    Cancelled = 6
}

public enum ProgramRegistrationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Attended = 4
}

public enum ProgramAttendanceStatus
{
    Present = 0,
    Absent = 1,
    Excused = 2
}

public enum ProgramSurveyStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

public enum ProgramCertificateIssueStatus
{
    Issued = 0,
    Cancelled = 1
}

public enum ProgramQualificationCaseStatus
{
    Required = 0,
    Opinion = 1,
    Approved = 2,
    Active = 3,
    Late = 4,
    Paid = 5,
    Completed = 6,
    Rejected = 7,
    Cancelled = 8
}

public enum ProgramQualificationInstallmentStatus
{
    Pending = 0,
    Late = 1,
    Paid = 2,
    Cancelled = 3
}

public class ProgramProject : IAuditable
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProjectType { get; set; } = "Project";
    public string? Description { get; set; }
    public string? ManagerName { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public ProgramProjectStatus Status { get; set; } = ProgramProjectStatus.Draft;
    public decimal Budget { get; set; }
    public decimal TargetBeneficiaries { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? RegistrationFormJson { get; set; }
    public string? SpecialProgramCategory { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<ProgramProjectTask> Tasks { get; set; } = new List<ProgramProjectTask>();
    public ICollection<ProgramProjectMilestone> Milestones { get; set; } = new List<ProgramProjectMilestone>();
    public ICollection<ProgramProjectContract> Contracts { get; set; } = new List<ProgramProjectContract>();
    public ICollection<ProgramProjectFinanceEntry> FinanceEntries { get; set; } = new List<ProgramProjectFinanceEntry>();
    public ICollection<ProgramProjectAssignment> Assignments { get; set; } = new List<ProgramProjectAssignment>();
    public ICollection<ProgramProjectReport> Reports { get; set; } = new List<ProgramProjectReport>();
    public ICollection<ProgramProjectActivity> Activities { get; set; } = new List<ProgramProjectActivity>();
    public ICollection<ProgramSupplierProposal> SupplierProposals { get; set; } = new List<ProgramSupplierProposal>();
}

public class ProgramProjectTask : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public DateTime? DueDate { get; set; }
    public ProgramProjectTaskStatus Status { get; set; } = ProgramProjectTaskStatus.New;
    public decimal ProgressPercent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramProjectMilestone : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal ProgressPercent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramProjectContract : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public int? ProgramSupplierId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ProgramSupplier? ProgramSupplier { get; set; }
}

public class ProgramProjectFinanceEntry : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public ProgramProjectFinanceEntryType EntryType { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public decimal Amount { get; set; }
    public string SourceOrPayee { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramProjectAssignment : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public ProgramProjectAssignmentType AssignmentType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramProjectReport : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string Summary { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramProjectActivity : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public ProgramProjectActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public decimal? Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramSupplier : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public ProgramSupplierStatus Status { get; set; } = ProgramSupplierStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<ProgramProjectContract> Contracts { get; set; } = new List<ProgramProjectContract>();
    public ICollection<ProgramSupplierProposal> Proposals { get; set; } = new List<ProgramSupplierProposal>();
}

public class ProgramSupplierProposal : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public int ProgramSupplierId { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Scope { get; set; }
    public decimal Amount { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? ValidUntil { get; set; }
    public ProgramSupplierProposalStatus Status { get; set; } = ProgramSupplierProposalStatus.Submitted;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public int? ConvertedContractId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ProgramSupplier? ProgramSupplier { get; set; }
    public ProgramProjectContract? ConvertedContract { get; set; }
}

public class ProgramIdea : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MarketingNotes { get; set; }
    public decimal EstimatedBudget { get; set; }
    public ProgramIdeaStatus Status { get; set; } = ProgramIdeaStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public int? ConvertedProjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<ProgramApproval> Approvals { get; set; } = new List<ProgramApproval>();
    public ProgramProject? ConvertedProject { get; set; }
}

public class ProgramApproval : IAuditable
{
    public int Id { get; set; }
    public int? ProgramIdeaId { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProgramApprovalStatus Status { get; set; } = ProgramApprovalStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramIdea? ProgramIdea { get; set; }
}

public class ProgramRegistration : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? ExternalReference { get; set; }
    public ProgramRegistrationStatus Status { get; set; } = ProgramRegistrationStatus.Pending;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
}

public class ProgramSession : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ICollection<ProgramSessionAttendance> AttendanceRecords { get; set; } = new List<ProgramSessionAttendance>();
}

public class ProgramSessionAttendance : IAuditable
{
    public int Id { get; set; }
    public int ProgramSessionId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public ProgramAttendanceStatus Status { get; set; } = ProgramAttendanceStatus.Present;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramSession? ProgramSession { get; set; }
}

public class ProgramSurvey : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string QuestionsJson { get; set; } = "[]";
    public ProgramSurveyStatus Status { get; set; } = ProgramSurveyStatus.Draft;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ICollection<ProgramSurveySubmission> Submissions { get; set; } = new List<ProgramSurveySubmission>();
}

public class ProgramSurveySubmission : IAuditable
{
    public int Id { get; set; }
    public int ProgramSurveyId { get; set; }
    public string RespondentName { get; set; } = string.Empty;
    public string AnswersJson { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramSurvey? ProgramSurvey { get; set; }
}

public class ProgramCertificateTemplate : IAuditable
{
    public int Id { get; set; }
    public int? ProgramProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ICollection<ProgramCertificateIssue> CertificateIssues { get; set; } = new List<ProgramCertificateIssue>();
}

public class ProgramCertificateIssue : IAuditable
{
    public int Id { get; set; }
    public int ProgramProjectId { get; set; }
    public int? ProgramCertificateTemplateId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public ProgramCertificateIssueStatus Status { get; set; } = ProgramCertificateIssueStatus.Issued;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ProgramCertificateTemplate? ProgramCertificateTemplate { get; set; }
}

public class ProgramQualificationCase : IAuditable
{
    public int Id { get; set; }
    public int? ProgramProjectId { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public string NeedSummary { get; set; } = string.Empty;
    public string? ManagementOpinion { get; set; }
    public ProgramQualificationCaseStatus Status { get; set; } = ProgramQualificationCaseStatus.Required;
    public decimal ApprovedAmount { get; set; }
    public int InstallmentCount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramProject? ProgramProject { get; set; }
    public ICollection<ProgramQualificationInstallment> Installments { get; set; } = new List<ProgramQualificationInstallment>();
}

public class ProgramQualificationInstallment : IAuditable
{
    public int Id { get; set; }
    public int ProgramQualificationCaseId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public ProgramQualificationInstallmentStatus Status { get; set; } = ProgramQualificationInstallmentStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ProgramQualificationCase? ProgramQualificationCase { get; set; }
}
