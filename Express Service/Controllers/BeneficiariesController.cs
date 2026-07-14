using Application.Contracts.Beneficiaries;
using Application.Service.Beneficiaries;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequirePermissionPrefix("system.beneficiary-accounts.")]
public class BeneficiariesController(IBeneficiaryService beneficiaries) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> SearchProfiles(
        [FromQuery] string? search,
        [FromQuery] BeneficiaryStatus? status,
        [FromQuery] string? category,
        [FromQuery] string? city,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.SearchAsync(new BeneficiarySearchRequest(search, status, category, city), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("profiles/{id:int}")]
    public async Task<IActionResult> GetProfile(int id, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("profiles")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateBeneficiaryRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("profiles/{id:int}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateBeneficiaryRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("profiles/{id:int}/archive")]
    public async Task<IActionResult> ArchiveProfile(int id, [FromBody] ArchiveBeneficiaryRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.ArchiveAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("profiles/{id:int}/restore")]
    public async Task<IActionResult> RestoreProfile(int id, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.RestoreAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("dependents")]
    public async Task<IActionResult> Dependents([FromQuery] int? beneficiaryProfileId, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetDependentsAsync(beneficiaryProfileId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("dependents")]
    public async Task<IActionResult> AddDependent([FromBody] AddBeneficiaryDependentRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.AddDependentAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("dependents/{id:int}")]
    public async Task<IActionResult> UpdateDependent(int id, [FromBody] UpdateBeneficiaryDependentRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateDependentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("guardians")]
    public async Task<IActionResult> Guardians([FromQuery] int? beneficiaryProfileId, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetGuardiansAsync(beneficiaryProfileId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("guardians")]
    public async Task<IActionResult> AddGuardian([FromBody] AddBeneficiaryGuardianRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.AddGuardianAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("guardians/{id:int}")]
    public async Task<IActionResult> UpdateGuardian(int id, [FromBody] UpdateBeneficiaryGuardianRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateGuardianAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("update-requests")]
    public async Task<IActionResult> UpdateRequests(
        [FromQuery] int? beneficiaryProfileId,
        [FromQuery] BeneficiaryUpdateRequestStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetUpdateRequestsAsync(beneficiaryProfileId, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("update-requests")]
    public async Task<IActionResult> CreateUpdateRequest([FromBody] CreateBeneficiaryUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CreateUpdateRequestAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("update-requests/{id:int}/decision")]
    public async Task<IActionResult> DecideUpdateRequest(int id, [FromBody] DecideBeneficiaryUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.DecideUpdateRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("update-requests/{id:int}/cancel")]
    public async Task<IActionResult> CancelUpdateRequest(int id, [FromBody] CancelBeneficiaryUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CancelUpdateRequestAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("entities")]
    public async Task<IActionResult> Entities(
        [FromQuery] string? search,
        [FromQuery] BeneficiaryEntityStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetEntitiesAsync(search, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("entities")]
    public async Task<IActionResult> CreateEntity([FromBody] UpsertBeneficiaryEntityRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.SaveEntityAsync(null, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("entities/{id:int}")]
    public async Task<IActionResult> UpdateEntity(int id, [FromBody] UpsertBeneficiaryEntityRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.SaveEntityAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("account-artifacts")]
    public async Task<IActionResult> AccountArtifacts(
        [FromQuery] BeneficiaryAccountArtifactType? type,
        [FromQuery] BeneficiaryAccountArtifactStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetAccountArtifactsAsync(type, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("account-artifacts")]
    public async Task<IActionResult> CreateAccountArtifact([FromBody] CreateBeneficiaryAccountArtifactRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CreateAccountArtifactAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("account-artifacts/{id:int}")]
    public async Task<IActionResult> UpdateAccountArtifact(int id, [FromBody] UpdateBeneficiaryAccountArtifactRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateAccountArtifactAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("account-artifacts/{id:int}/status")]
    public async Task<IActionResult> UpdateAccountArtifactStatus(int id, [FromBody] UpdateBeneficiaryAccountArtifactStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateAccountArtifactStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("guardian-operations")]
    public async Task<IActionResult> GuardianOperations(
        [FromQuery] BeneficiaryGuardianOperationType? type,
        [FromQuery] BeneficiaryOperationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetGuardianOperationsAsync(type, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("guardian-operations")]
    public async Task<IActionResult> CreateGuardianOperation([FromBody] CreateBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CreateGuardianOperationAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("guardian-operations/{id:int}/decision")]
    public async Task<IActionResult> DecideGuardianOperation(int id, [FromBody] DecideBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.DecideGuardianOperationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("guardian-operations/{id:int}/cancel")]
    public async Task<IActionResult> CancelGuardianOperation(int id, [FromBody] CancelBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CancelGuardianOperationAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("update-batches")]
    public async Task<IActionResult> UpdateBatches(
        [FromQuery] BeneficiaryUpdateBatchKind? kind,
        [FromQuery] BeneficiaryOperationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await beneficiaries.GetUpdateBatchesAsync(kind, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("update-batches")]
    public async Task<IActionResult> CreateUpdateBatch([FromBody] CreateBeneficiaryUpdateBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.CreateUpdateBatchAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("update-batches/{id:int}")]
    public async Task<IActionResult> UpdateUpdateBatch(int id, [FromBody] UpdateBeneficiaryUpdateBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateUpdateBatchAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("update-batches/{id:int}/progress")]
    public async Task<IActionResult> UpdateBatchProgress(int id, [FromBody] UpdateBeneficiaryBatchProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await beneficiaries.UpdateBatchProgressAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
