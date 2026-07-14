using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

public sealed class PermissionRequirement(string key) : IAuthorizationRequirement
{
    public string Key { get; } = key;
}
