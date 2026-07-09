using Application.Abstraction;
using Application.Contracts.ExecutiveSupervision;
using Domain.Entities;

namespace Application.Service.ExecutiveSupervision;

public interface IExecutiveSupervisionService
{
    Task<Result<ExecutiveSupervisionDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EstablishmentDocumentResponse>>> GetFoundationDocumentsAsync(EstablishmentDocumentStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<EstablishmentDocumentResponse>> SaveFoundationDocumentAsync(int? id, SaveEstablishmentDocumentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AidCommitteeCreditEntryResponse>>> GetAidCommitteeEntriesAsync(CancellationToken cancellationToken = default);
    Task<Result<AidCommitteeCreditEntryResponse>> SaveAidCommitteeEntryAsync(int? id, SaveAidCommitteeCreditEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ExecutiveApprovalRequestResponse>>> GetApprovalsAsync(ExecutiveApprovalKind? kind = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<ExecutiveApprovalRequestResponse>> SaveApprovalAsync(int? id, SaveExecutiveApprovalRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result> DecideApprovalAsync(int id, DecideExecutiveWorkflowRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PaymentAuthorizationResponse>>> GetPaymentAuthorizationsAsync(ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<PaymentAuthorizationResponse>> SavePaymentAuthorizationAsync(int? id, SavePaymentAuthorizationRequest request, CancellationToken cancellationToken = default);
    Task<Result> DecidePaymentAuthorizationAsync(int id, DecideExecutiveWorkflowRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AdministrativeDecisionRecordResponse>>> GetAdministrativeDecisionsAsync(AdministrativeDecisionType? type = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<AdministrativeDecisionRecordResponse>> SaveAdministrativeDecisionAsync(int? id, SaveAdministrativeDecisionRecordRequest request, CancellationToken cancellationToken = default);
}
