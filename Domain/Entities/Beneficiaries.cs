using Domain.Auditing;

namespace Domain.Entities;

public enum BeneficiaryStatus
{
    Active = 0,
    Suspended = 1,
    Archived = 2,
    Deleted = 3
}

public enum BeneficiaryUpdateRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum BeneficiaryEntityStatus
{
    Pending = 0,
    Active = 1,
    Inactive = 2,
    Archived = 3
}

public enum BeneficiaryAccountArtifactType
{
    Card = 0,
    Barcode = 1,
    ExternalJoin = 2,
    AssociationSearch = 3
}

public enum BeneficiaryAccountArtifactStatus
{
    Draft = 0,
    Ready = 1,
    Issued = 2,
    Cancelled = 3
}

public enum BeneficiaryGuardianOperationType
{
    ConvertBeneficiaryToGuardian = 0,
    ConvertGuardianToBeneficiary = 1,
    RemoveGuardian = 2
}

public enum BeneficiaryOperationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}

public enum BeneficiaryUpdateBatchKind
{
    BulkUpdate = 0,
    SelfService = 1,
    AidRequest = 2,
    ManualStatus = 3,
    ReportNoUpdate = 4,
    ReportWrongUpdate = 5,
    ReportSelfUpdate = 6,
    ReportFieldUpdate = 7
}

public class BeneficiaryProfile : IAuditable
{
    public int Id { get; set; }
    public string BeneficiaryNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Category { get; set; }
    public string? Grade { get; set; }
    public BeneficiaryStatus Status { get; set; } = BeneficiaryStatus.Active;
    public decimal MonthlyIncome { get; set; }
    public int FamilyMembersCount { get; set; }
    public string? Notes { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ArchiveReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<BeneficiaryDependent> Dependents { get; set; } = new List<BeneficiaryDependent>();
    public ICollection<BeneficiaryGuardian> Guardians { get; set; } = new List<BeneficiaryGuardian>();
    public ICollection<BeneficiaryUpdateRequest> UpdateRequests { get; set; } = new List<BeneficiaryUpdateRequest>();
    public ICollection<BeneficiaryAccountArtifact> AccountArtifacts { get; set; } = new List<BeneficiaryAccountArtifact>();
    public ICollection<BeneficiaryGuardianOperation> GuardianOperations { get; set; } = new List<BeneficiaryGuardianOperation>();
}

public class BeneficiaryDependent : IAuditable
{
    public int Id { get; set; }
    public int BeneficiaryProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Category { get; set; }
    public string? Grade { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
}

public class BeneficiaryGuardian : IAuditable
{
    public int Id { get; set; }
    public int BeneficiaryProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Mobile { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeleteReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
    public ICollection<BeneficiaryGuardianOperation> Operations { get; set; } = new List<BeneficiaryGuardianOperation>();
}

public class BeneficiaryUpdateRequest : IAuditable
{
    public int Id { get; set; }
    public int BeneficiaryProfileId { get; set; }
    public string RequestedField { get; set; } = string.Empty;
    public string? CurrentValue { get; set; }
    public string? RequestedValue { get; set; }
    public string? Reason { get; set; }
    public BeneficiaryUpdateRequestStatus Status { get; set; } = BeneficiaryUpdateRequestStatus.Pending;
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
}

public class BeneficiaryEntity : IAuditable
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public BeneficiaryEntityStatus Status { get; set; } = BeneficiaryEntityStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class BeneficiaryAccountArtifact : IAuditable
{
    public int Id { get; set; }
    public BeneficiaryAccountArtifactType Type { get; set; }
    public BeneficiaryAccountArtifactStatus Status { get; set; } = BeneficiaryAccountArtifactStatus.Draft;
    public int? BeneficiaryProfileId { get; set; }
    public int? BeneficiaryDependentId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? Payload { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
    public BeneficiaryDependent? BeneficiaryDependent { get; set; }
}

public class BeneficiaryGuardianOperation : IAuditable
{
    public int Id { get; set; }
    public BeneficiaryGuardianOperationType Type { get; set; }
    public BeneficiaryOperationStatus Status { get; set; } = BeneficiaryOperationStatus.Pending;
    public int? BeneficiaryProfileId { get; set; }
    public int? BeneficiaryGuardianId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BeneficiaryProfile? BeneficiaryProfile { get; set; }
    public BeneficiaryGuardian? BeneficiaryGuardian { get; set; }
}

public class BeneficiaryUpdateBatch : IAuditable
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public BeneficiaryUpdateBatchKind Kind { get; set; }
    public BeneficiaryOperationStatus Status { get; set; } = BeneficiaryOperationStatus.Pending;
    public string? AssignedTo { get; set; }
    public int TotalProfiles { get; set; }
    public int CompletedProfiles { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
