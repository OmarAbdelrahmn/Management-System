using Application.Contracts.ExecutiveSupervision;
using Application.Service.ExecutiveSupervision;
using Domain.Entities;

namespace Express_Service.Services;

public class ExecutiveSupervisionUiService(IExecutiveSupervisionService service)
{
    public async Task<ExecutiveSupervisionDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }
    public async Task<List<EstablishmentDocumentResponse>> GetFoundationDocumentsAsync(EstablishmentDocumentStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetFoundationDocumentsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveFoundationDocumentAsync(int? id, SaveEstablishmentDocumentRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveFoundationDocumentAsync(id, request, cancellationToken), "تم حفظ مستند التأسيس.");
    public async Task<List<AidCommitteeCreditEntryResponse>> GetAidCommitteeEntriesAsync(CancellationToken cancellationToken = default) => ToList(await service.GetAidCommitteeEntriesAsync(cancellationToken));
    public async Task<(bool Success, string Message)> SaveAidCommitteeEntryAsync(int? id, SaveAidCommitteeCreditEntryRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveAidCommitteeEntryAsync(id, request, cancellationToken), "تم حفظ حركة رصيد لجنة المساعدات.");
    public async Task<List<ExecutiveApprovalRequestResponse>> GetApprovalsAsync(ExecutiveApprovalKind? kind = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetApprovalsAsync(kind, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveApprovalAsync(int? id, SaveExecutiveApprovalRequestRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveApprovalAsync(id, request, cancellationToken), "تم حفظ طلب الاعتماد.");
    public async Task<(bool Success, string Message)> DecideApprovalAsync(int id, ExecutiveWorkflowStatus status, string? note = null, CancellationToken cancellationToken = default) => ToUi(await service.DecideApprovalAsync(id, new DecideExecutiveWorkflowRequest(status, note), cancellationToken), "تم تحديث الاعتماد.");
    public async Task<List<PaymentAuthorizationResponse>> GetPaymentAuthorizationsAsync(ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetPaymentAuthorizationsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SavePaymentAuthorizationAsync(int? id, SavePaymentAuthorizationRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SavePaymentAuthorizationAsync(id, request, cancellationToken), "تم حفظ تعميد الصرف.");
    public async Task<(bool Success, string Message)> DecidePaymentAuthorizationAsync(int id, ExecutiveWorkflowStatus status, string? note = null, CancellationToken cancellationToken = default) => ToUi(await service.DecidePaymentAuthorizationAsync(id, new DecideExecutiveWorkflowRequest(status, note), cancellationToken), "تم تحديث تعميد الصرف.");
    public async Task<List<AdministrativeDecisionRecordResponse>> GetAdministrativeDecisionsAsync(AdministrativeDecisionType? type = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetAdministrativeDecisionsAsync(type, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveAdministrativeDecisionAsync(int? id, SaveAdministrativeDecisionRecordRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveAdministrativeDecisionAsync(id, request, cancellationToken), "تم حفظ القرار الإداري.");

    private static (bool Success, string Message) ToUi(Application.Abstraction.Result result, string successMessage) => result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) => result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);
    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) => result.IsSuccess ? result.Value.ToList() : [];
}
