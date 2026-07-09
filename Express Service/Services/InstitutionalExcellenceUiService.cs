using Application.Contracts.InstitutionalExcellence;
using Application.Service.InstitutionalExcellence;
using Domain.Entities;

namespace Express_Service.Services;

public class InstitutionalExcellenceUiService(IInstitutionalExcellenceService service)
{
    public async Task<InstitutionalExcellenceDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<PerformanceMeasureResponse>> GetPerformanceMeasuresAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetPerformanceMeasuresAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SavePerformanceMeasureAsync(int? id, SavePerformanceMeasureRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SavePerformanceMeasureAsync(id, request, cancellationToken), "تم حفظ مقياس الأداء.");
    public async Task<List<GovernanceCycleResponse>> GetGovernanceCyclesAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetGovernanceCyclesAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveGovernanceCycleAsync(int? id, SaveGovernanceCycleRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveGovernanceCycleAsync(id, request, cancellationToken), "تم حفظ دورة الحوكمة.");
    public async Task<List<GovernanceCriterionResponse>> GetGovernanceCriteriaAsync(int? cycleId = null, GovernanceCriterionStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetGovernanceCriteriaAsync(cycleId, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveGovernanceCriterionAsync(int? id, SaveGovernanceCriterionRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveGovernanceCriterionAsync(id, request, cancellationToken), "تم حفظ معيار الحوكمة.");
    public async Task<List<GovernanceAttachmentResponse>> GetGovernanceAttachmentsAsync(int? criterionId = null, CancellationToken cancellationToken = default) => ToList(await service.GetGovernanceAttachmentsAsync(criterionId, cancellationToken));
    public async Task<(bool Success, string Message)> SaveGovernanceAttachmentAsync(int? id, SaveGovernanceAttachmentRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveGovernanceAttachmentAsync(id, request, cancellationToken), "تم حفظ مرفق التحقق.");
    public async Task<List<GovernanceTaskResponse>> GetGovernanceTasksAsync(int? cycleId = null, GovernanceTaskStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetGovernanceTasksAsync(cycleId, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveGovernanceTaskAsync(int? id, SaveGovernanceTaskRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveGovernanceTaskAsync(id, request, cancellationToken), "تم حفظ مهمة الحوكمة.");
    public async Task<GovernanceReportResponse?> GetGovernanceReportAsync(int? cycleId = null, CancellationToken cancellationToken = default)
    {
        var result = await service.GetGovernanceReportAsync(cycleId, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }
    public async Task<List<StrategicPlanResponse>> GetStrategicPlansAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetStrategicPlansAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveStrategicPlanAsync(int? id, SaveStrategicPlanRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveStrategicPlanAsync(id, request, cancellationToken), "تم حفظ الخطة الإستراتيجية.");
    public async Task<List<StrategicPerspectiveResponse>> GetStrategicPerspectivesAsync(int? planId = null, CancellationToken cancellationToken = default) => ToList(await service.GetStrategicPerspectivesAsync(planId, cancellationToken));
    public async Task<(bool Success, string Message)> SaveStrategicPerspectiveAsync(int? id, SaveStrategicPerspectiveRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveStrategicPerspectiveAsync(id, request, cancellationToken), "تم حفظ منظور الخطة.");
    public async Task<List<StrategicGoalResponse>> GetStrategicGoalsAsync(int? perspectiveId = null, CancellationToken cancellationToken = default) => ToList(await service.GetStrategicGoalsAsync(perspectiveId, cancellationToken));
    public async Task<(bool Success, string Message)> SaveStrategicGoalAsync(int? id, SaveStrategicGoalRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveStrategicGoalAsync(id, request, cancellationToken), "تم حفظ الهدف الإستراتيجي.");
    public async Task<List<StrategicIndicatorResponse>> GetStrategicIndicatorsAsync(int? planId = null, StrategicIndicatorKind? kind = null, CancellationToken cancellationToken = default) => ToList(await service.GetStrategicIndicatorsAsync(planId, kind, cancellationToken));
    public async Task<(bool Success, string Message)> SaveStrategicIndicatorAsync(int? id, SaveStrategicIndicatorRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveStrategicIndicatorAsync(id, request, cancellationToken), "تم حفظ المؤشر الإستراتيجي.");
    public async Task<List<StrategicVariableResponse>> GetStrategicVariablesAsync(int? planId = null, CancellationToken cancellationToken = default) => ToList(await service.GetStrategicVariablesAsync(planId, cancellationToken));
    public async Task<(bool Success, string Message)> SaveStrategicVariableAsync(int? id, SaveStrategicVariableRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveStrategicVariableAsync(id, request, cancellationToken), "تم حفظ متغير الخطة.");
    public async Task<List<StrategicVariableResponse>> FetchAutomatedStrategicVariablesAsync(int planId, CancellationToken cancellationToken = default) => ToList(await service.FetchAutomatedStrategicVariablesAsync(planId, cancellationToken));

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
