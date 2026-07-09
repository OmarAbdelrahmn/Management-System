using Application.Contracts.Admin;
using Application.Service.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var result = await adminService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateUserAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        var result = await adminService.GetRolesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.CreateRoleAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions(CancellationToken cancellationToken)
    {
        var result = await adminService.GetPermissionsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("permissions")]
    public async Task<IActionResult> SavePermission([FromBody] SavePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SavePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("role-permissions")]
    public async Task<IActionResult> RolePermissions(CancellationToken cancellationToken)
    {
        var result = await adminService.GetRolePermissionsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("role-permissions")]
    public async Task<IActionResult> GrantRolePermission([FromBody] GrantRolePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.GrantRolePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("login-audit")]
    public async Task<IActionResult> LoginAudit(CancellationToken cancellationToken)
    {
        var result = await adminService.GetLoginAuditAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var result = await adminService.GetSettingsAsync(category, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSetting([FromBody] SaveSystemSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SaveSettingAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("files")]
    public async Task<IActionResult> Files(CancellationToken cancellationToken)
    {
        var result = await adminService.GetFilesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("files")]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.SaveFileAssetAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("audit-log")]
    public async Task<IActionResult> AuditLog(CancellationToken cancellationToken)
    {
        var result = await adminService.GetAuditLogsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("jobs/status")]
    public IActionResult JobsStatus()
    {
        var result = adminService.GetJobDashboardStatus(
            configuration.GetValue("Hangfire:Enabled", false),
            configuration["Hangfire:EmailRecurringCron"] ?? "*/5 * * * *");
        return Ok(result.Value);
    }
}
