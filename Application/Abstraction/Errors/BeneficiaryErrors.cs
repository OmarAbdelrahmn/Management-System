using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class BeneficiaryErrors
{
    public static readonly Error ProfileNotFound =
        new("Beneficiaries.ProfileNotFound", "Beneficiary profile was not found.", StatusCodes.Status404NotFound);

    public static readonly Error UpdateRequestNotFound =
        new("Beneficiaries.UpdateRequestNotFound", "Beneficiary update request was not found.", StatusCodes.Status404NotFound);

    public static readonly Error EntityNotFound =
        new("Beneficiaries.EntityNotFound", "Beneficiary entity was not found.", StatusCodes.Status404NotFound);

    public static readonly Error GuardianNotFound =
        new("Beneficiaries.GuardianNotFound", "Beneficiary guardian was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DependentNotFound =
        new("Beneficiaries.DependentNotFound", "Beneficiary dependent was not found.", StatusCodes.Status404NotFound);

    public static readonly Error AccountArtifactNotFound =
        new("Beneficiaries.AccountArtifactNotFound", "Beneficiary account artifact was not found.", StatusCodes.Status404NotFound);

    public static readonly Error GuardianOperationNotFound =
        new("Beneficiaries.GuardianOperationNotFound", "Beneficiary guardian operation was not found.", StatusCodes.Status404NotFound);

    public static readonly Error UpdateBatchNotFound =
        new("Beneficiaries.UpdateBatchNotFound", "Beneficiary update batch was not found.", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateBeneficiaryNumber =
        new("Beneficiaries.DuplicateBeneficiaryNumber", "Beneficiary number is already used.", StatusCodes.Status409Conflict);

    public static readonly Error DuplicateEntityName =
        new("Beneficiaries.DuplicateEntityName", "Beneficiary entity name is already used.", StatusCodes.Status409Conflict);

    public static readonly Error UpdateRequestAlreadyDecided =
        new("Beneficiaries.UpdateRequestAlreadyDecided", "Beneficiary update request has already been decided.", StatusCodes.Status409Conflict);

    public static readonly Error OperationAlreadyDecided =
        new("Beneficiaries.OperationAlreadyDecided", "Beneficiary operation has already been decided.", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRequest =
        new("Beneficiaries.InvalidRequest", "Beneficiary request is invalid.", StatusCodes.Status400BadRequest);
}
