using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class ProgramsProjectsErrors
{
    public static readonly Error ProjectNotFound =
        new("ProgramsProjects.ProjectNotFound", "Project or program was not found.", StatusCodes.Status404NotFound);

    public static readonly Error TaskNotFound =
        new("ProgramsProjects.TaskNotFound", "Project task was not found.", StatusCodes.Status404NotFound);

    public static readonly Error MilestoneNotFound =
        new("ProgramsProjects.MilestoneNotFound", "Project milestone was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ContractNotFound =
        new("ProgramsProjects.ContractNotFound", "Project contract was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SupplierNotFound =
        new("ProgramsProjects.SupplierNotFound", "Supplier was not found.", StatusCodes.Status404NotFound);

    public static readonly Error IdeaNotFound =
        new("ProgramsProjects.IdeaNotFound", "Program idea was not found.", StatusCodes.Status404NotFound);

    public static readonly Error ApprovalNotFound =
        new("ProgramsProjects.ApprovalNotFound", "Program approval was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RegistrationNotFound =
        new("ProgramsProjects.RegistrationNotFound", "Program registration was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SessionNotFound =
        new("ProgramsProjects.SessionNotFound", "Program session was not found.", StatusCodes.Status404NotFound);

    public static readonly Error SurveyNotFound =
        new("ProgramsProjects.SurveyNotFound", "Program survey was not found.", StatusCodes.Status404NotFound);

    public static readonly Error CertificateTemplateNotFound =
        new("ProgramsProjects.CertificateTemplateNotFound", "Program certificate template was not found.", StatusCodes.Status404NotFound);

    public static readonly Error QualificationCaseNotFound =
        new("ProgramsProjects.QualificationCaseNotFound", "Qualification case was not found.", StatusCodes.Status404NotFound);

    public static readonly Error QualificationInstallmentNotFound =
        new("ProgramsProjects.QualificationInstallmentNotFound", "Qualification installment was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateProjectCode =
        new("ProgramsProjects.DuplicateProjectCode", "Project code is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateSupplierName =
        new("ProgramsProjects.DuplicateSupplierName", "Supplier name is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateContractNumber =
        new("ProgramsProjects.DuplicateContractNumber", "Contract number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateCertificateNumber =
        new("ProgramsProjects.DuplicateCertificateNumber", "Certificate number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRequest =
        new("ProgramsProjects.InvalidRequest", "Programs and projects request is invalid.", StatusCodes.Status400BadRequest);
}
