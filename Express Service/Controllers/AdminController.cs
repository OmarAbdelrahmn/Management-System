using Application.Contracts.Admin;
using Application.Service.Admin;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdminController(IAdminService adminService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("users")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var result = await adminService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("users")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("users/{id}")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateUserAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("roles")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        var result = await adminService.GetRolesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("roles")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.CreateRoleAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("roles/{id}")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateRoleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("roles/{id}")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> DeleteRole(string id, CancellationToken cancellationToken)
    {
        var result = await adminService.DeleteRoleAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("permissions")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> Permissions(CancellationToken cancellationToken)
    {
        var result = await adminService.GetPermissionsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("permissions")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> SavePermission([FromBody] SavePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SavePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("role-permissions")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> RolePermissions(CancellationToken cancellationToken)
    {
        var result = await adminService.GetRolePermissionsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("role-permissions")]
    [RequirePermission("system.tech-enablement.system_permissions")]
    public async Task<IActionResult> GrantRolePermission([FromBody] GrantRolePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.GrantRolePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("login-audit")]
    [RequirePermission("system.tech-enablement.system_register")]
    public async Task<IActionResult> LoginAudit(CancellationToken cancellationToken)
    {
        var result = await adminService.GetLoginAuditAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("settings")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> Settings([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var result = await adminService.GetSettingsAsync(category, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("settings")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> SaveSetting([FromBody] SaveSystemSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SaveSettingAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("files")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> Files([FromQuery] string? search, [FromQuery] string? category, [FromQuery] bool? isPublic, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? sortBy, [FromQuery] bool descending = true, CancellationToken cancellationToken = default)
    {
        return LegacyFilesRetired();
    }

    [HttpPost("files")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileAssetRequest request, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpPost("files/upload")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public IActionResult UploadFile()
    {
        return LegacyFilesRetired();
    }

    [HttpGet("files/{id:int}/download")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> DownloadFile(int id, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpPost("files/links")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> LinkFile([FromBody] LinkFileAssetRequest request, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpGet("files/links")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> FileLinks([FromQuery] string entityType, [FromQuery] string entityId, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpDelete("files/links/{linkId:int}")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> UnlinkFile(int linkId, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpPut("files/{id:int}")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> UpdateFile(int id, [FromBody] UpdateFileAssetRequest request, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpDelete("files/{id:int}")]
    [RequirePermission("system.tech-enablement.system_administrators")]
    public async Task<IActionResult> DeleteFile(int id, CancellationToken cancellationToken)
    {
        return LegacyFilesRetired();
    }

    [HttpGet("audit-log")]
    [RequirePermission("system.tech-enablement.system_register")]
    public async Task<IActionResult> AuditLog([FromQuery] string? search, [FromQuery] string? actorUserId, [FromQuery] string? entityName, [FromQuery] string? entityId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? sortBy, [FromQuery] bool descending = true, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetAuditLogsAsync(new AuditLogSearchRequest(search, actorUserId, entityName, entityId, from, to, page, pageSize, sortBy, descending), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("query-views")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> QueryViews([FromQuery] string screenKey, CancellationToken cancellationToken)
    {
        var result = await adminService.GetQueryViewsAsync(screenKey, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("query-views")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> SaveQueryView([FromBody] SaveQueryViewRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SaveQueryViewAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("query-views/{id:int}")]
    [RequirePermission("system.tech-enablement.system_settings")]
    public async Task<IActionResult> DeleteQueryView(int id, CancellationToken cancellationToken)
    {
        var result = await adminService.DeleteQueryViewAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("jobs/status")]
    [RequirePermission("system.tech-enablement.system_worker_log")]
    public IActionResult JobsStatus()
    {
        var result = adminService.GetJobDashboardStatus(
            configuration.GetValue("Hangfire:Enabled", false),
            configuration["Hangfire:EmailRecurringCron"] ?? "*/5 * * * *");
        return Ok(result.Value);
    }

    private ObjectResult LegacyFilesRetired() => StatusCode(StatusCodes.Status410Gone, new ProblemDetails
    {
        Status = StatusCodes.Status410Gone,
        Title = "Legacy file API retired",
        Detail = "Use /api/attachments through an authorized linked business record.",
        Type = "/api/attachments"
    });
}
