using Domain.Entities;

namespace Application.Contracts.TechEnablement;

public record TechEnablementDashboardResponse(int SettingsCount, int OrganizationAssignmentsCount, int VisualAssetsCount, int OpenSecurityReviewsCount, int NcnpNeedsUpdateCount, int NcnpReadyCount);

public record TechSystemSettingResponse(int Id, string Key, string NameAr, string Category, string? Value, string? Notes, string Status);
public record SaveTechSystemSettingRequest(string Key, string NameAr, string Category, string? Value, string? Notes, TechSettingStatus Status);

public record OrganizationAssignmentResponse(int Id, string UnitName, string AssigneeName, string AssignmentType, string? RoleTitle, DateTime EffectiveFrom, DateTime? EffectiveTo, bool IsActive, string? Notes);
public record SaveOrganizationAssignmentRequest(string UnitName, string AssigneeName, OrganizationAssignmentType AssignmentType, string? RoleTitle, DateTime EffectiveFrom, DateTime? EffectiveTo, bool IsActive, string? Notes);

public record VisualAssetTemplateResponse(int Id, string Name, string AssetType, string? FileUrl, string? DesignJson, bool IsActive, string? Notes);
public record SaveVisualAssetTemplateRequest(string Name, VisualAssetType AssetType, string? FileUrl, string? DesignJson, bool IsActive, string? Notes);

public record CybersecurityReviewResponse(int Id, string Area, string Finding, string Severity, string Status, string? Owner, DateTime? DueDate, string? MitigationPlan);
public record SaveCybersecurityReviewRequest(string Area, string Finding, string Severity, CybersecurityReviewStatus Status, string? Owner, DateTime? DueDate, string? MitigationPlan);

public record NcnpDataRecordResponse(int Id, string ReferenceNumber, string BeneficiaryName, string SupportType, DateTime? SupportDate, decimal Cost, string Status, string? PlatformReference, string? Notes);
public record SaveNcnpDataRecordRequest(string ReferenceNumber, string BeneficiaryName, string SupportType, DateTime? SupportDate, decimal Cost, NcnpDataStatus Status, string? PlatformReference, string? Notes);
public record UpdateNcnpDataStatusRequest(NcnpDataStatus Status, string? PlatformReference, string? Notes);
