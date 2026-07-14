using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Express_Service.Authorization;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "permission:";
    public const string PrefixPolicyPrefix = "permission-prefix:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PrefixPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = policyName[PrefixPolicyPrefix.Length..];
            return string.IsNullOrWhiteSpace(prefix)
                ? null
                : new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new PermissionPrefixRequirement(prefix)).Build();
        }

        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase)) return await base.GetPolicyAsync(policyName);

        var key = policyName[PolicyPrefix.Length..];
        return string.IsNullOrWhiteSpace(key)
            ? null
            : new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(key)).Build();
    }
}
