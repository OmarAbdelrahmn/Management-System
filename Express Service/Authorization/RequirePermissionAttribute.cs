using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute(string permissionKey) : AuthorizeAttribute(PermissionPolicyProvider.PolicyPrefix + permissionKey)
{
    public string PermissionKey { get; } = permissionKey;
}
