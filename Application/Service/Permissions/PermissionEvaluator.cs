using Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Service.Permissions;

public class PermissionEvaluator(ApplicationDbcontext dbcontext) : IPermissionEvaluator
{
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal principal, string permissionKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionKey) || principal.Identity?.IsAuthenticated != true)
            return false;

        var roles = principal.FindAll(ClaimTypes.Role).Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            return true;
        if (roles.Length == 0)
            return false;

        return await dbcontext.RolePermissions.AsNoTracking()
            .AnyAsync(x => x.IsGranted && x.AppPermission != null && x.Role != null &&
                x.AppPermission.Key == permissionKey && roles.Contains(x.Role.Name!), cancellationToken);
    }

    public async Task<bool> HasPermissionPrefixAsync(ClaimsPrincipal principal, string permissionPrefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionPrefix) || principal.Identity?.IsAuthenticated != true)
            return false;

        var roles = principal.FindAll(ClaimTypes.Role).Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            return true;
        if (roles.Length == 0)
            return false;

        return await dbcontext.RolePermissions.AsNoTracking()
            .AnyAsync(x => x.IsGranted && x.AppPermission != null && x.Role != null &&
                x.AppPermission.Key.StartsWith(permissionPrefix) && roles.Contains(x.Role.Name!), cancellationToken);
    }
}
