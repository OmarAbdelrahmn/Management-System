using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionPrefixAttribute(string permissionPrefix) : AuthorizeAttribute(PermissionPolicyProvider.PrefixPolicyPrefix + permissionPrefix)
{
    public string PermissionPrefix { get; } = permissionPrefix;
}
