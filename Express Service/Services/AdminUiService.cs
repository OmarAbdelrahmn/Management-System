using Application.Contracts.Admin;
using Application.Service.Admin;
using Domain.Entities;

namespace Express_Service.Services;

public class AdminUiService(IAdminService adminService, IConfiguration configuration)
{
    public async Task<List<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء المستخدم.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(string id, UpdateAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.UpdateUserAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث المستخدم.") : (false, result.Error.Description);
    }

    public async Task<List<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetRolesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        var result = await adminService.CreateRoleAsync(new CreateRoleRequest(name), cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء الدور.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateRoleAsync(string id, string name, CancellationToken cancellationToken = default)
    {
        var result = await adminService.UpdateRoleAsync(id, new UpdateRoleRequest(name), cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث الدور.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await adminService.DeleteRoleAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تم حذف الدور.") : (false, result.Error.Description);
    }

    public async Task<List<PermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetPermissionsAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<RolePermissionResponse>> GetRolePermissionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetRolePermissionsAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SavePermissionAsync(SavePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.SavePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الصلاحية.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> GrantPermissionAsync(GrantRolePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GrantRolePermissionAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث صلاحية الدور.") : (false, result.Error.Description);
    }

    public async Task<List<LoginAuditResponse>> GetLoginAuditAsync(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetLoginAuditAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<SystemSettingResponse>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetSettingsAsync(category, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveSettingAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.SaveSettingAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ الإعداد.") : (false, result.Error.Description);
    }

    public async Task<PagedResponse<FileAssetResponse>> GetFilesAsync(string? search = null, string? category = null, bool? isPublic = null, int page = 1, int pageSize = 25, string? sortBy = null, bool descending = true, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetFilesAsync(new FileAssetSearchRequest(search, category, isPublic, page, pageSize, sortBy, descending), cancellationToken);
        return result.IsSuccess ? result.Value : new PagedResponse<FileAssetResponse>([], 1, pageSize, 0, 0);
    }

    public async Task<(bool Success, string Message)> SaveFileAsync(SaveFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.SaveFileAssetAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ بيانات الملف.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateFileAsync(int id, UpdateFileAssetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.UpdateFileAssetAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث بيانات الملف.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DeleteFileAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await adminService.DeleteFileAssetAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تم حذف سجل الملف.") : (false, result.Error.Description);
    }

    public async Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(string? search = null, string? actorUserId = null, string? entityName = null, string? entityId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 25, string? sortBy = null, bool descending = true, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetAuditLogsAsync(new AuditLogSearchRequest(search, actorUserId, entityName, entityId, from, to, page, pageSize, sortBy, descending), cancellationToken);
        return result.IsSuccess ? result.Value : new PagedResponse<AuditLogResponse>([], 1, pageSize, 0, 0);
    }

    public async Task<List<QueryViewResponse>> GetQueryViewsAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetQueryViewsAsync(screenKey, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> SaveQueryViewAsync(SaveQueryViewRequest request, CancellationToken cancellationToken = default)
    {
        var result = await adminService.SaveQueryViewAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم حفظ العرض.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DeleteQueryViewAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await adminService.DeleteQueryViewAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تم حذف العرض.") : (false, result.Error.Description);
    }

    public JobDashboardStatusResponse GetJobsStatus() =>
        adminService.GetJobDashboardStatus(
            configuration.GetValue("Hangfire:Enabled", false),
            configuration["Hangfire:EmailRecurringCron"] ?? "*/5 * * * *").Value;
}
