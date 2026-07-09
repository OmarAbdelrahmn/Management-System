using Application.Contracts.TechEnablement;
using Application.Service.TechEnablement;
using Domain.Entities;

namespace Express_Service.Services;

public class TechEnablementUiService(ITechEnablementService service)
{
    public async Task<TechEnablementDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<TechSystemSettingResponse>> GetSettingsAsync(string? category = null, TechSettingStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetSettingsAsync(category, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveSettingAsync(int? id, SaveTechSystemSettingRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveSettingAsync(id, request, cancellationToken), "تم حفظ إعداد التمكين التقني.");

    public async Task<List<OrganizationAssignmentResponse>> GetOrganizationAssignmentsAsync(OrganizationAssignmentType? assignmentType = null, bool? isActive = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetOrganizationAssignmentsAsync(assignmentType, isActive, cancellationToken));
    public async Task<(bool Success, string Message)> SaveOrganizationAssignmentAsync(int? id, SaveOrganizationAssignmentRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveOrganizationAssignmentAsync(id, request, cancellationToken), "تم حفظ تعيين الهيكل التنظيمي.");

    public async Task<List<VisualAssetTemplateResponse>> GetVisualAssetsAsync(VisualAssetType? assetType = null, bool? isActive = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetVisualAssetsAsync(assetType, isActive, cancellationToken));
    public async Task<(bool Success, string Message)> SaveVisualAssetAsync(int? id, SaveVisualAssetTemplateRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveVisualAssetAsync(id, request, cancellationToken), "تم حفظ قالب التصميم.");

    public async Task<List<CybersecurityReviewResponse>> GetCybersecurityReviewsAsync(CybersecurityReviewStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetCybersecurityReviewsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveCybersecurityReviewAsync(int? id, SaveCybersecurityReviewRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveCybersecurityReviewAsync(id, request, cancellationToken), "تم حفظ سجل الأمن السيبراني.");

    public async Task<List<NcnpDataRecordResponse>> GetNcnpDataAsync(NcnpDataStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetNcnpDataAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveNcnpDataAsync(int? id, SaveNcnpDataRecordRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveNcnpDataAsync(id, request, cancellationToken), "تم حفظ سجل المركز الوطني.");
    public async Task<(bool Success, string Message)> UpdateNcnpStatusAsync(int id, NcnpDataStatus status, string? platformReference = null, string? notes = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.UpdateNcnpStatusAsync(id, new UpdateNcnpDataStatusRequest(status, platformReference, notes), cancellationToken), "تم تحديث حالة سجل المركز الوطني.");

    private static (bool Success, string Message) ToUi(Application.Abstraction.Result result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
