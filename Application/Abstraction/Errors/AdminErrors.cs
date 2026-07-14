using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class AdminErrors
{
    public static readonly Error UserNotFound =
        new("Admin.UserNotFound", "User was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RoleNotFound =
        new("Admin.RoleNotFound", "Role was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RoleInUse =
        new("Admin.RoleInUse", "Role is assigned to one or more users and cannot be deleted.", StatusCodes.Status409Conflict);

    public static readonly Error PermissionNotFound =
        new("Admin.PermissionNotFound", "Permission was not found.", StatusCodes.Status404NotFound);

    public static readonly Error FileNotFound =
        new("Admin.FileNotFound", "File asset was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRequest =
        new("Admin.InvalidRequest", "Admin request is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error IdentityFailure =
        new("Admin.IdentityFailure", "Identity operation failed.", StatusCodes.Status400BadRequest);
}
