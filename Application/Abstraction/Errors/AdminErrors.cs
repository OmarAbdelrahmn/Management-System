using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class AdminErrors
{
    public static readonly Error UserNotFound =
        new("Admin.UserNotFound", "User was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RoleNotFound =
        new("Admin.RoleNotFound", "Role was not found.", StatusCodes.Status404NotFound);

    public static readonly Error PermissionNotFound =
        new("Admin.PermissionNotFound", "Permission was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRequest =
        new("Admin.InvalidRequest", "Admin request is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error IdentityFailure =
        new("Admin.IdentityFailure", "Identity operation failed.", StatusCodes.Status400BadRequest);
}
