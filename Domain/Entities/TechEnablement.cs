using Domain.Auditing;

namespace Domain.Entities;

public enum TechSettingStatus
{
    Active = 0,
    Inactive = 1
}

public enum OrganizationAssignmentType
{
    Position = 0,
    Committee = 1,
    Signature = 2,
    Cycle = 3
}

public enum VisualAssetType
{
    DashboardTheme = 0,
    PrintTemplate = 1,
    GiftCard = 2,
    Greeting = 3,
    PdfDocument = 4,
    Certificate = 5
}

public enum CybersecurityReviewStatus
{
    Open = 0,
    InReview = 1,
    Mitigated = 2,
    Accepted = 3
}

public enum NcnpDataStatus
{
    NeedsUpdate = 0,
    ReadyToRegister = 1,
    Registered = 2,
    RemovedOrSuspended = 3,
    ArchivedExternalSupport = 4
}

public class TechSystemSetting : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Value { get; set; }
    public string? Notes { get; set; }
    public TechSettingStatus Status { get; set; } = TechSettingStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class OrganizationAssignment : IAuditable
{
    public int Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string AssigneeName { get; set; } = string.Empty;
    public OrganizationAssignmentType AssignmentType { get; set; } = OrganizationAssignmentType.Position;
    public string? RoleTitle { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class VisualAssetTemplate : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public VisualAssetType AssetType { get; set; } = VisualAssetType.PrintTemplate;
    public string? FileUrl { get; set; }
    public string? DesignJson { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class CybersecurityReview : IAuditable
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Finding { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public CybersecurityReviewStatus Status { get; set; } = CybersecurityReviewStatus.Open;
    public string? Owner { get; set; }
    public DateTime? DueDate { get; set; }
    public string? MitigationPlan { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class NcnpDataRecord : IAuditable
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string SupportType { get; set; } = string.Empty;
    public DateTime? SupportDate { get; set; }
    public decimal Cost { get; set; }
    public NcnpDataStatus Status { get; set; } = NcnpDataStatus.NeedsUpdate;
    public string? PlatformReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
