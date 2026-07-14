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
    Task<Result<RoleResponse>> UpdateRoleAsync(string id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PermissionResponse>>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Result<PermissionResponse>> SavePermissionAsync(SavePermissionRequest request, CancellationToken cancellationToken = default);
    Task<Result> GrantRolePermissionAsync(GrantRolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RolePermissionResponse>>> GetRolePermissionsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LoginAuditResponse>>> GetLoginAuditAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SystemSettingResponse>>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<Result<SystemSettingResponse>> SaveSettingAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResponse<FileAssetResponse>>> GetFilesAsync(FileAssetSearchRequest? request = null, CancellationToken cancellationToken = default);
    Task<Result<FileAssetResponse>> SaveFileAssetAsync(SaveFileAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<FileAssetResponse>> UpdateFileAssetAsync(int id, UpdateFileAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<FileAssetResponse>> GetFileAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> DeleteFileAssetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<FileAssetLinkResponse>> LinkFileAsync(LinkFileAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileAssetLinkResponse>>> GetFileLinksAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<Result> UnlinkFileAsync(int linkId, CancellationToken cancellationToken = default);
    Task<Result<PagedResponse<AuditLogResponse>>> GetAuditLogsAsync(AuditLogSearchRequest? request = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<QueryViewResponse>>> GetQueryViewsAsync(string screenKey, CancellationToken cancellationToken = default);
    Task<Result<QueryViewResponse>> SaveQueryViewAsync(SaveQueryViewRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteQueryViewAsync(int id, CancellationToken cancellationToken = default);
    Result<JobDashboardStatusResponse> GetJobDashboardStatus(bool hangfireEnabled, string cron);
}
