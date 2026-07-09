using Application.Contracts.ExecutiveSupervision;
using Application.Service.ExecutiveSupervision;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class ExecutiveSupervisionController(IExecutiveSupervisionService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("foundation")]
    public async Task<IActionResult> Foundation([FromQuery] EstablishmentDocumentStatus? status, CancellationToken ct) => ToAction(await service.GetFoundationDocumentsAsync(status, ct));
    [HttpPost("foundation")]
    public async Task<IActionResult> SaveFoundation([FromBody] SaveEstablishmentDocumentRequest request, CancellationToken ct) => ToAction(await service.SaveFoundationDocumentAsync(null, request, ct));
    [HttpGet("aid-committee")]
    public async Task<IActionResult> AidCommittee(CancellationToken ct) => ToAction(await service.GetAidCommitteeEntriesAsync(ct));
    [HttpPost("aid-committee")]
    public async Task<IActionResult> SaveAidCommittee([FromBody] SaveAidCommitteeCreditEntryRequest request, CancellationToken ct) => ToAction(await service.SaveAidCommitteeEntryAsync(null, request, ct));
    [HttpGet("approvals")]
    public async Task<IActionResult> Approvals([FromQuery] ExecutiveApprovalKind? kind, [FromQuery] ExecutiveWorkflowStatus? status, CancellationToken ct) => ToAction(await service.GetApprovalsAsync(kind, status, ct));
    [HttpPost("approvals")]
    public async Task<IActionResult> SaveApproval([FromBody] SaveExecutiveApprovalRequestRequest request, CancellationToken ct) => ToAction(await service.SaveApprovalAsync(null, request, ct));
    [HttpPost("approvals/{id:int}/decide")]
    public async Task<IActionResult> DecideApproval(int id, [FromBody] DecideExecutiveWorkflowRequest request, CancellationToken ct)
    {
        var result = await service.DecideApprovalAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("authorizations")]
    public async Task<IActionResult> Authorizations([FromQuery] ExecutiveWorkflowStatus? status, CancellationToken ct) => ToAction(await service.GetPaymentAuthorizationsAsync(status, ct));
    [HttpPost("authorizations")]
    public async Task<IActionResult> SaveAuthorization([FromBody] SavePaymentAuthorizationRequest request, CancellationToken ct) => ToAction(await service.SavePaymentAuthorizationAsync(null, request, ct));
    [HttpPost("authorizations/{id:int}/decide")]
    public async Task<IActionResult> DecideAuthorization(int id, [FromBody] DecideExecutiveWorkflowRequest request, CancellationToken ct)
    {
        var result = await service.DecidePaymentAuthorizationAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("decisions")]
    public async Task<IActionResult> Decisions([FromQuery] AdministrativeDecisionType? type, [FromQuery] ExecutiveWorkflowStatus? status, CancellationToken ct) => ToAction(await service.GetAdministrativeDecisionsAsync(type, status, ct));
    [HttpPost("decisions")]
    public async Task<IActionResult> SaveDecision([FromBody] SaveAdministrativeDecisionRecordRequest request, CancellationToken ct) => ToAction(await service.SaveAdministrativeDecisionAsync(null, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
