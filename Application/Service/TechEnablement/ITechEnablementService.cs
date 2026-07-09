using Application.Abstraction;
using Application.Contracts.TechEnablement;
using Domain.Entities;

namespace Application.Service.TechEnablement;

public interface ITechEnablementService
{
    Task<Result<TechEnablementDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TechSystemSettingResponse>>> GetSettingsAsync(string? category = null, TechSettingStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<TechSystemSettingResponse>> SaveSettingAsync(int? id, SaveTechSystemSettingRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OrganizationAssignmentResponse>>> GetOrganizationAssignmentsAsync(OrganizationAssignmentType? assignmentType = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<Result<OrganizationAssignmentResponse>> SaveOrganizationAssignmentAsync(int? id, SaveOrganizationAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VisualAssetTemplateResponse>>> GetVisualAssetsAsync(VisualAssetType? assetType = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<Result<VisualAssetTemplateResponse>> SaveVisualAssetAsync(int? id, SaveVisualAssetTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CybersecurityReviewResponse>>> GetCybersecurityReviewsAsync(CybersecurityReviewStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<CybersecurityReviewResponse>> SaveCybersecurityReviewAsync(int? id, SaveCybersecurityReviewRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NcnpDataRecordResponse>>> GetNcnpDataAsync(NcnpDataStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<NcnpDataRecordResponse>> SaveNcnpDataAsync(int? id, SaveNcnpDataRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateNcnpStatusAsync(int id, UpdateNcnpDataStatusRequest request, CancellationToken cancellationToken = default);
}
