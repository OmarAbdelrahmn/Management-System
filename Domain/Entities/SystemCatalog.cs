using Domain.Auditing;

namespace Domain.Entities;

public enum SystemPageStatus
{
    Planned = 0,
    InProgress = 1,
    Implemented = 2,
    Deferred = 3
}

public class SystemModule : IAuditable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<SystemPage> Pages { get; set; } = new List<SystemPage>();
}

public class SystemPage : IAuditable
{
    public int Id { get; set; }
    public int SystemModuleId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServicePlan { get; set; } = string.Empty;
    public string UiPlan { get; set; } = string.Empty;
    public SystemPageStatus Status { get; set; } = SystemPageStatus.Planned;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public SystemModule? SystemModule { get; set; }
}
