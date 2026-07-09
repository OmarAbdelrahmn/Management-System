using Domain.Entities;

namespace Application.Contracts.Beneficiaries;

public record BeneficiaryDashboardResponse(
    int ProfilesCount,
    int ActiveProfilesCount,
    int ArchivedProfilesCount,
    int DependentsCount,
    int GuardiansCount,
    int PendingUpdateRequestsCount,
    int BeneficiaryEntitiesCount,
    int AccountArtifactsCount,
    int PendingGuardianOperationsCount,
    int OpenUpdateBatchesCount);

public record BeneficiaryResponse(
    int Id,
    string BeneficiaryNumber,
    string FullName,
    string? NationalId,
    string? Gender,
    DateTime? BirthDate,
    string? Mobile,
    string? Email,
    string? City,
    string? Address,
    string? Category,
    string? Grade,
    string Status,
    decimal MonthlyIncome,
    int FamilyMembersCount,
    string? Notes,
    DateTime? ArchivedAt,
    string? ArchiveReason,
    DateTime CreatedAt);

public record CreateBeneficiaryRequest(
    string FullName,
    string? BeneficiaryNumber,
    string? NationalId,
    string? Gender,
    DateTime? BirthDate,
    string? Mobile,
    string? Email,
    string? City,
    string? Address,
    string? Category,
    string? Grade,
    decimal MonthlyIncome,
    int FamilyMembersCount,
    string? Notes);

public record UpdateBeneficiaryRequest(
    string FullName,
    string? NationalId,
    string? Gender,
    DateTime? BirthDate,
    string? Mobile,
    string? Email,
    string? City,
    string? Address,
    string? Category,
    string? Grade,
    BeneficiaryStatus Status,
    decimal MonthlyIncome,
    int FamilyMembersCount,
    string? Notes);

public record BeneficiarySearchRequest(
    string? Search,
    BeneficiaryStatus? Status,
    string? Category,
    string? City);

public record ArchiveBeneficiaryRequest(string Reason);

public record BeneficiaryDependentResponse(
    int Id,
    int BeneficiaryProfileId,
    string BeneficiaryName,
    string FullName,
    string? NationalId,
    string Relationship,
    DateTime? BirthDate,
    string? Category,
    string? Grade,
    bool IsActive,
    string? Notes);

public record AddBeneficiaryDependentRequest(
    int BeneficiaryProfileId,
    string FullName,
    string? NationalId,
    string Relationship,
    DateTime? BirthDate,
    string? Category,
    string? Grade,
    string? Notes);

public record BeneficiaryGuardianResponse(
    int Id,
    int BeneficiaryProfileId,
    string BeneficiaryName,
    string FullName,
    string? NationalId,
    string? Mobile,
    string Relationship,
    bool IsPrimary,
    bool IsDeleted,
    DateTime? DeletedAt,
    string? DeleteReason,
    string? Notes);

public record AddBeneficiaryGuardianRequest(
    int BeneficiaryProfileId,
    string FullName,
    string? NationalId,
    string? Mobile,
    string Relationship,
    bool IsPrimary,
    string? Notes);

public record BeneficiaryUpdateRequestResponse(
    int Id,
    int BeneficiaryProfileId,
    string BeneficiaryName,
    string RequestedField,
    string? CurrentValue,
    string? RequestedValue,
    string? Reason,
    string Status,
    string? DecisionNotes,
    DateTime? DecidedAt,
    DateTime CreatedAt);

public record CreateBeneficiaryUpdateRequest(
    int BeneficiaryProfileId,
    string RequestedField,
    string? CurrentValue,
    string? RequestedValue,
    string? Reason);

public record DecideBeneficiaryUpdateRequest(
    bool Approved,
    string? Notes);

public record BeneficiaryEntityResponse(
    int Id,
    string NameAr,
    string? NameEn,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? City,
    string? Address,
    string Status,
    string? Notes);

public record UpsertBeneficiaryEntityRequest(
    string NameAr,
    string? NameEn,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? City,
    string? Address,
    BeneficiaryEntityStatus Status,
    string? Notes);

public record BeneficiaryAccountArtifactResponse(
    int Id,
    string Type,
    string Status,
    int? BeneficiaryProfileId,
    int? BeneficiaryDependentId,
    string ReferenceNumber,
    string HolderName,
    string? Source,
    string? Payload,
    string? Notes,
    DateTime CreatedAt);

public record CreateBeneficiaryAccountArtifactRequest(
    BeneficiaryAccountArtifactType Type,
    int? BeneficiaryProfileId,
    int? BeneficiaryDependentId,
    string? HolderName,
    string? Source,
    string? Payload,
    string? Notes);

public record UpdateBeneficiaryAccountArtifactStatusRequest(
    BeneficiaryAccountArtifactStatus Status,
    string? Notes);

public record BeneficiaryGuardianOperationResponse(
    int Id,
    string Type,
    string Status,
    int? BeneficiaryProfileId,
    int? BeneficiaryGuardianId,
    string ReferenceNumber,
    string SubjectName,
    string? Notes,
    string? DecisionNotes,
    DateTime? DecidedAt,
    DateTime CreatedAt);

public record CreateBeneficiaryGuardianOperationRequest(
    BeneficiaryGuardianOperationType Type,
    int? BeneficiaryProfileId,
    int? BeneficiaryGuardianId,
    string? SubjectName,
    string? Notes);

public record DecideBeneficiaryGuardianOperationRequest(
    bool Approved,
    string? DecisionNotes);

public record BeneficiaryUpdateBatchResponse(
    int Id,
    string BatchNumber,
    string Title,
    string Kind,
    string Status,
    string? AssignedTo,
    int TotalProfiles,
    int CompletedProfiles,
    DateTime? DueDate,
    string? Notes,
    DateTime CreatedAt);

public record CreateBeneficiaryUpdateBatchRequest(
    BeneficiaryUpdateBatchKind Kind,
    string Title,
    string? AssignedTo,
    int TotalProfiles,
    DateTime? DueDate,
    string? Notes);

public record UpdateBeneficiaryBatchProgressRequest(
    BeneficiaryOperationStatus Status,
    int CompletedProfiles,
    string? Notes);
