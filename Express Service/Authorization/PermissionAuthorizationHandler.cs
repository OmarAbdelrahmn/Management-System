using Application.Service.Permissions;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Express_Service.Authorization;

public sealed class PermissionAuthorizationHandler(IPermissionEvaluator evaluator, ApplicationDbcontext dbcontext) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await evaluator.HasPermissionAsync(context.User, requirement.Key))
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
                EntityName = "Permission",
                EntityId = requirement.Key,
                Details = $"{httpContext.Request.Method} {httpContext.Request.Path}; ip={httpContext.Connection.RemoteIpAddress ?? System.Net.IPAddress.None}"
            });
            await dbcontext.SaveChangesAsync(httpContext.RequestAborted);
        }
    }
}
