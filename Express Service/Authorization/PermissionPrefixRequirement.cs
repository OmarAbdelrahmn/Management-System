using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

public sealed class PermissionPrefixRequirement(string prefix) : IAuthorizationRequirement
{
    public string Prefix { get; } = prefix;
}
