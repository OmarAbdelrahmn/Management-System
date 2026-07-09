using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class TechEnablementErrors
{
    public static readonly Error InvalidRequest = new("TechEnablement.InvalidRequest", "Technical enablement request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error SettingNotFound = new("TechEnablement.SettingNotFound", "Technical setting was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateSettingKey = new("TechEnablement.DuplicateSettingKey", "Technical setting key is already used.", StatusCodes.Status409Conflict);
    public static readonly Error OrganizationAssignmentNotFound = new("TechEnablement.OrganizationAssignmentNotFound", "Organization assignment was not found.", StatusCodes.Status404NotFound);
    public static readonly Error VisualAssetNotFound = new("TechEnablement.VisualAssetNotFound", "Visual asset template was not found.", StatusCodes.Status404NotFound);
    public static readonly Error SecurityReviewNotFound = new("TechEnablement.SecurityReviewNotFound", "Cybersecurity review was not found.", StatusCodes.Status404NotFound);
    public static readonly Error NcnpRecordNotFound = new("TechEnablement.NcnpRecordNotFound", "NCNP data record was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateNcnpReference = new("TechEnablement.DuplicateNcnpReference", "NCNP reference number is already used.", StatusCodes.Status409Conflict);
}
