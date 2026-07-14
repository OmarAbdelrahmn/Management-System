using Application.Service.Permissions;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

public sealed class PermissionPrefixAuthorizationHandler(IPermissionEvaluator evaluator, ApplicationDbcontext dbcontext) : AuthorizationHandler<PermissionPrefixRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionPrefixRequirement requirement)
    {
        if (await evaluator.HasPermissionPrefixAsync(context.User, requirement.Prefix))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is HttpContext httpContext && context.User.Identity?.IsAuthenticated == true)
        {
            dbcontext.AuditLogs.Add(new AuditLog
            {
                ActorUserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                Action = "PermissionDenied",
                EntityName = "PermissionPrefix",
                EntityId = requirement.Prefix,
                Details = $"{httpContext.Request.Method} {httpContext.Request.Path}; ip={httpContext.Connection.RemoteIpAddress ?? System.Net.IPAddress.None}"
            });
            await dbcontext.SaveChangesAsync(httpContext.RequestAborted);
        }
    }
}
