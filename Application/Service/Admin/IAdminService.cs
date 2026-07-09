using Application.Abstraction;
using Application.Contracts.Admin;

namespace Application.Service.Admin;

public interface IAdminService
{
    Task<Result<IEnumerable<AdminUserResponse>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> UpdateUserAsync(string id, UpdateAdminUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RoleResponse>>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PermissionResponse>>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Result<PermissionResponse>> SavePermissionAsync(SavePermissionRequest request, CancellationToken cancellationToken = default);
    Task<Result> GrantRolePermissionAsync(GrantRolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RolePermissionResponse>>> GetRolePermissionsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LoginAuditResponse>>> GetLoginAuditAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemSettingResponse>>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<Result<SystemSettingResponse>> SaveSettingAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileAssetResponse>>> GetFilesAsync(CancellationToken cancellationToken = default);
    Task<Result<FileAssetResponse>> SaveFileAssetAsync(SaveFileAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AuditLogResponse>>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
    Result<JobDashboardStatusResponse> GetJobDashboardStatus(bool hangfireEnabled, string cron);
}
