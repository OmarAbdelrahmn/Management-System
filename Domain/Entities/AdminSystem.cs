using Domain.Auditing;
using Domain.Identity;

namespace Domain.Entities;

public enum SystemSettingValueType
{
    Text = 0,
    Number = 1,
    Boolean = 2,
    Secret = 3
}

public enum LoginAuditResult
{
    Success = 0,
    Failed = 1,
    LockedOut = 2,
    Inactive = 3
}

public class AppPermission : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : IAuditable
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public int AppPermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationRole? Role { get; set; }
    public AppPermission? AppPermission { get; set; }
}

public class UserLoginAudit : IAuditable
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public LoginAuditResult Result { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationUser? User { get; set; }
}

public class SystemSetting : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Value { get; set; }
    public SystemSettingValueType ValueType { get; set; } = SystemSettingValueType.Text;
    public string Category { get; set; } = string.Empty;
    public bool IsEditable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class FileAsset : IAuditable
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? PurgeAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ApplicationUser? UploadedByUser { get; set; }
    public ICollection<FileAssetLink> Links { get; set; } = new List<FileAssetLink>();
    public ICollection<FileAssetVersion> Versions { get; set; } = new List<FileAssetVersion>();
}

public enum FileAssetScanStatus
{
    Clean = 0,
    Infected = 1,
    Unavailable = 2,
    Failed = 3
}

public class FileAssetVersion : IAuditable
{
    public int Id { get; set; }
    public int FileAssetId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public FileAssetScanStatus ScanStatus { get; set; }
    public DateTime? ScannedAt { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FileAsset? FileAsset { get; set; }
    public ApplicationUser? UploadedByUser { get; set; }
}

public class FileAssetLink : IAuditable
{
    public int Id { get; set; }
    public int FileAssetId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FileAsset? FileAsset { get; set; }
}

public class SavedQueryView : IAuditable
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ScreenKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FilterJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
