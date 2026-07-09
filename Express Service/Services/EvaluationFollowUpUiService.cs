using Application.Contracts.EvaluationFollowUp;
using Application.Service.EvaluationFollowUp;
using Domain.Entities;

namespace Express_Service.Services;

public class EvaluationFollowUpUiService(IEvaluationFollowUpService service)
{
    public async Task<EvaluationFollowUpDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<FollowUpCaseResponse>> GetCasesAsync(FollowUpCaseStatus? status = null, FollowUpSubjectType? subjectType = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetCasesAsync(status, subjectType, cancellationToken));

    public async Task<(bool Success, string Message)> SaveCaseAsync(int? id, SaveFollowUpCaseRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveCaseAsync(id, request, cancellationToken), "تم حفظ متابعة الحالة.");

    public async Task<(bool Success, string Message)> UpdateCaseStatusAsync(int id, FollowUpCaseStatus status, string? note = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.UpdateCaseStatusAsync(id, new UpdateFollowUpCaseStatusRequest(status, note), cancellationToken), "تم تحديث حالة المتابعة.");

    public async Task<List<FollowUpActivityResponse>> GetActivitiesAsync(int? caseId = null, FollowUpSubjectType? subjectType = null, bool nextActionsOnly = false, CancellationToken cancellationToken = default) =>
        ToList(await service.GetActivitiesAsync(caseId, subjectType, nextActionsOnly, cancellationToken));

    public async Task<(bool Success, string Message)> SaveActivityAsync(int? id, SaveFollowUpActivityRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveActivityAsync(id, request, cancellationToken), "تم حفظ نشاط المتابعة.");

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
