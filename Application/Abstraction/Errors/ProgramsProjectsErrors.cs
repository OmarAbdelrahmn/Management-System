using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class ProgramsProjectsErrors
{
    public static readonly Error ProjectNotFound =
        new("ProgramsProjects.ProjectNotFound", "Project or program was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ProjectAlreadyClosed =
        new("ProgramsProjects.ProjectAlreadyClosed", "Project or program is already closed.", StatusCodes.Status409Conflict);

    public static readonly Error ProjectFinalReportRequired =
        new("ProgramsProjects.ProjectFinalReportRequired", "Project or program completion requires the final-report close workflow.", StatusCodes.Status400BadRequest);

    public static readonly Error ProjectStatusNotesRequired =
        new("ProgramsProjects.ProjectStatusNotesRequired", "Project status notes are required for cancelled or deleted projects.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidProjectStatusTransition =
        new("ProgramsProjects.InvalidProjectStatusTransition", "Project or program status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error TaskNotFound =
        new("ProgramsProjects.TaskNotFound", "Project task was not found.", StatusCodes.Status404NotFound);

    public static readonly Error TaskCompletionProgressRequired =
        new("ProgramsProjects.TaskCompletionProgressRequired", "Completed project tasks must have 100 percent progress.", StatusCodes.Status400BadRequest);

    public static readonly Error MilestoneNotFound =
        new("ProgramsProjects.MilestoneNotFound", "Project milestone was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ContractNotFound =
        new("ProgramsProjects.ContractNotFound", "Project contract was not found.", StatusCodes.Status404NotFound);

    public static readonly Error FinanceEntryNotFound =
        new("ProgramsProjects.FinanceEntryNotFound", "Project finance entry was not found.", StatusCodes.Status404NotFound);

    public static readonly Error AssignmentNotFound =
        new("ProgramsProjects.AssignmentNotFound", "Project assignment was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ReportNotFound =
        new("ProgramsProjects.ReportNotFound", "Project report was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SupplierNotFound =
        new("ProgramsProjects.SupplierNotFound", "Supplier was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SupplierProposalNotFound =
        new("ProgramsProjects.SupplierProposalNotFound", "Supplier proposal was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SupplierProposalAlreadyClosed =
        new("ProgramsProjects.SupplierProposalAlreadyClosed", "Supplier proposal is already closed or converted.", StatusCodes.Status409Conflict);

    public static readonly Error SupplierProposalExpired =
        new("ProgramsProjects.SupplierProposalExpired", "Supplier proposal validity date has expired.", StatusCodes.Status409Conflict);

    public static readonly Error SupplierProposalDecisionNotesRequired =
        new("ProgramsProjects.SupplierProposalDecisionNotesRequired", "Decision notes are required for rejected or cancelled supplier proposals.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidSupplierProposalStatusTransition =
        new("ProgramsProjects.InvalidSupplierProposalStatusTransition", "Supplier proposal status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error IdeaNotFound =
        new("ProgramsProjects.IdeaNotFound", "Program idea was not found.", StatusCodes.Status404NotFound);

    public static readonly Error IdeaNotApproved =
        new("ProgramsProjects.IdeaNotApproved", "Program idea must be approved before conversion.", StatusCodes.Status400BadRequest);

    public static readonly Error IdeaAlreadyConverted =
        new("ProgramsProjects.IdeaAlreadyConverted", "Program idea has already been converted to a project.", StatusCodes.Status409Conflict);

    public static readonly Error ApprovalNotFound =
        new("ProgramsProjects.ApprovalNotFound", "Program approval was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RegistrationNotFound =
        new("ProgramsProjects.RegistrationNotFound", "Program registration was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ProgramCapacityReached =
        new("ProgramsProjects.ProgramCapacityReached", "Program registration capacity has been reached.", StatusCodes.Status409Conflict);

    public static readonly Error RegistrationAlreadyDecided =
        new("ProgramsProjects.RegistrationAlreadyDecided", "Program registration has already been decided.", StatusCodes.Status409Conflict);

    public static readonly Error RegistrationDecisionNotesRequired =
        new("ProgramsProjects.RegistrationDecisionNotesRequired", "Decision notes are required for rejected or cancelled program registrations.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRegistrationStatusTransition =
        new("ProgramsProjects.InvalidRegistrationStatusTransition", "Program registration status transition is invalid.", StatusCodes.Status409Conflict);

    public static readonly Error SessionNotFound =
        new("ProgramsProjects.SessionNotFound", "Program session was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SurveyNotFound =
        new("ProgramsProjects.SurveyNotFound", "Program survey was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SurveyNotActive =
        new("ProgramsProjects.SurveyNotActive", "Program survey must be active before accepting submissions.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidSurveyJson =
        new("ProgramsProjects.InvalidSurveyJson", "Program survey JSON payload is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateSurveySubmission =
        new("ProgramsProjects.DuplicateSurveySubmission", "Survey respondent has already submitted an answer.", StatusCodes.Status409Conflict);

    public static readonly Error CertificateTemplateNotFound =
        new("ProgramsProjects.CertificateTemplateNotFound", "Program certificate template was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CertificateIssueNotFound =
        new("ProgramsProjects.CertificateIssueNotFound", "Program certificate issue was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CertificateIssueAlreadyCancelled =
        new("ProgramsProjects.CertificateIssueAlreadyCancelled", "Program certificate issue is already cancelled.", StatusCodes.Status409Conflict);

    public static readonly Error QualificationCaseNotFound =
        new("ProgramsProjects.QualificationCaseNotFound", "Qualification case was not found.", StatusCodes.Status404NotFound);

    public static readonly Error QualificationCaseNotApproved =
        new("ProgramsProjects.QualificationCaseNotApproved", "Qualification case must be approved before installment generation.", StatusCodes.Status400BadRequest);

    public static readonly Error QualificationInstallmentNotFound =
        new("ProgramsProjects.QualificationInstallmentNotFound", "Qualification installment was not found.", StatusCodes.Status404NotFound);

    public static readonly Error QualificationInstallmentsAlreadyGenerated =
        new("ProgramsProjects.QualificationInstallmentsAlreadyGenerated", "Qualification installments have already been generated.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateProjectCode =
        new("ProgramsProjects.DuplicateProjectCode", "Project code is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateSupplierName =
        new("ProgramsProjects.DuplicateSupplierName", "Supplier name is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateContractNumber =
        new("ProgramsProjects.DuplicateContractNumber", "Contract number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error ContractAlreadyPaid =
        new("ProgramsProjects.ContractAlreadyPaid", "Project contract is already fully paid.", StatusCodes.Status409Conflict);

    public static readonly Error ContractPaymentExceedsBalance =
        new("ProgramsProjects.ContractPaymentExceedsBalance", "Contract payment exceeds the remaining contract balance.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateProposalNumber =
        new("ProgramsProjects.DuplicateProposalNumber", "Supplier proposal number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateCertificateNumber =
        new("ProgramsProjects.DuplicateCertificateNumber", "Certificate number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error CertificateRequiresAttendedRegistration =
        new("ProgramsProjects.CertificateRequiresAttendedRegistration", "Certificate can only be issued from an attended program registration.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRequest =
        new("ProgramsProjects.InvalidRequest", "Programs and projects request is invalid.", StatusCodes.Status400BadRequest);
}
