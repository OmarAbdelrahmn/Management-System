using Domain.Auditing;

namespace Domain.Entities;

public enum ArchiveDocumentCategory
{
    Corporate = 0,
    General = 1,
    Finance = 2,
    Budget = 3,
    Admin = 4,
    Secret = 5
}

public enum ArchiveDocumentStatus
{
    Active = 0,
    Archived = 1,
    Restricted = 2,
    Removed = 3
}

public enum CorrespondenceDirection
{
    Outgoing = 0,
    Incoming = 1
}

public enum CorrespondenceStatus
{
    Draft = 0,
    Registered = 1,
    NeedsReply = 2,
    Replied = 3,
    Completed = 4,
    Removed = 5,
    PendingRemovalApproval = 6
}

public enum CorrespondenceOperationStatus
{
    Open = 0,
    Completed = 1,
    Cancelled = 2
}

public class ArchiveDocument : IAuditable
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ArchiveDocumentCategory Category { get; set; } = ArchiveDocumentCategory.General;
    public string? FileUrl { get; set; }
    public string? OwnerDepartment { get; set; }
    public ArchiveDocumentStatus Status { get; set; } = ArchiveDocumentStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class CorrespondenceRecord : IAuditable
{
    public int Id { get; set; }
    public string MailNumber { get; set; } = string.Empty;
    public CorrespondenceDirection Direction { get; set; } = CorrespondenceDirection.Outgoing;
    public string Subject { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public DateTime MailDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? BarcodeValue { get; set; }
    public CorrespondenceStatus Status { get; set; } = CorrespondenceStatus.Registered;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<CorrespondenceOperation> Operations { get; set; } = new List<CorrespondenceOperation>();
}

public class CorrespondenceOperation : IAuditable
{
    public int Id { get; set; }
    public int CorrespondenceRecordId { get; set; }
    public string OperationNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public CorrespondenceOperationStatus Status { get; set; } = CorrespondenceOperationStatus.Open;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public CorrespondenceRecord? CorrespondenceRecord { get; set; }
}
