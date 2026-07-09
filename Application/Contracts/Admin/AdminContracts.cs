using Domain.Entities;

namespace Application.Contracts.Admin;

public record AdminUserResponse(
    string Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);

public record CreateAdminUserRequest(string FullName, string Email, string Password, string? PhoneNumber, IReadOnlyList<string> Roles);

public record UpdateAdminUserRequest(string FullName, string? PhoneNumber, bool IsActive, IReadOnlyList<string> Roles);

public record RoleResponse(string Id, string Name);

public record CreateRoleRequest(string Name);

public record PermissionResponse(int Id, string Key, string NameAr, string Category, string? Description);

public record RolePermissionResponse(string RoleId, string RoleName, int PermissionId, string PermissionKey, string PermissionNameAr, bool IsGranted);

public record SavePermissionRequest(string Key, string NameAr, string Category, string? Description);

public record GrantRolePermissionRequest(string RoleId, int PermissionId, bool IsGranted);

public record LoginAuditResponse(
    int Id,
    string? UserId,
    string UserName,
    string Result,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent,
    DateTime AttemptedAt);

public record SystemSettingResponse(
    int Id,
    string Key,
    string NameAr,
    string? Value,
    string ValueType,
    string Category,
    bool IsEditable);

public record SaveSystemSettingRequest(string Key, string NameAr, string? Value, SystemSettingValueType ValueType, string Category, bool IsEditable = true);

public record FileAssetResponse(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    string Category,
    string UploadedByUserId,
    string UploadedByName,
    bool IsPublic,
    DateTime CreatedAt);

public record SaveFileAssetRequest(string OriginalFileName, string ContentType, long SizeBytes, string StoragePath, string Category, bool IsPublic);

public record AuditLogResponse(int Id, string ActorUserId, string Action, string EntityName, string EntityId, string Details, DateTime CreatedAt);

public record JobDashboardStatusResponse(bool HangfireEnabled, string DashboardPath, string EmailRecurringCron);
