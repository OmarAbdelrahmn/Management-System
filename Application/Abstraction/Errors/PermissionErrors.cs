using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class PermissionErrors
{
    public static readonly Error Forbidden =
        new("Permission.Forbidden", "Current user does not have permission to perform this board action.", StatusCodes.Status403Forbidden);

    public static readonly Error UserRequired =
        new("Permission.UserRequired", "Authenticated user context is required for this action.", StatusCodes.Status401Unauthorized);
}
