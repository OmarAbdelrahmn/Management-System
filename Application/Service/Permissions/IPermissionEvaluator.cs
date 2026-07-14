using System.Security.Claims;

namespace Application.Service.Permissions;

public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal principal, string permissionKey, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionPrefixAsync(ClaimsPrincipal principal, string permissionPrefix, CancellationToken cancellationToken = default);
}
