using Application.Abstraction;
using Application.Contracts.Beneficiaries;
using Domain.Entities;

namespace Application.Service.Beneficiaries;

public interface IBeneficiaryService
{
    Task<Result<BeneficiaryDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryResponse>>> SearchAsync(BeneficiarySearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryResponse>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryResponse>> CreateAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryResponse>> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(int id, ArchiveBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryDependentResponse>>> GetDependentsAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryDependentResponse>> AddDependentAsync(AddBeneficiaryDependentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryGuardianResponse>>> GetGuardiansAsync(int? beneficiaryProfileId = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryGuardianResponse>> AddGuardianAsync(AddBeneficiaryGuardianRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryUpdateRequestResponse>>> GetUpdateRequestsAsync(int? beneficiaryProfileId = null, BeneficiaryUpdateRequestStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryUpdateRequestResponse>> CreateUpdateRequestAsync(CreateBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryUpdateRequestResponse>> DecideUpdateRequestAsync(int id, DecideBeneficiaryUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryEntityResponse>>> GetEntitiesAsync(string? search = null, BeneficiaryEntityStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryEntityResponse>> SaveEntityAsync(int? id, UpsertBeneficiaryEntityRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryAccountArtifactResponse>>> GetAccountArtifactsAsync(BeneficiaryAccountArtifactType? type = null, BeneficiaryAccountArtifactStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryAccountArtifactResponse>> CreateAccountArtifactAsync(CreateBeneficiaryAccountArtifactRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryAccountArtifactResponse>> UpdateAccountArtifactStatusAsync(int id, UpdateBeneficiaryAccountArtifactStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryGuardianOperationResponse>>> GetGuardianOperationsAsync(BeneficiaryGuardianOperationType? type = null, BeneficiaryOperationStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryGuardianOperationResponse>> CreateGuardianOperationAsync(CreateBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryGuardianOperationResponse>> DecideGuardianOperationAsync(int id, DecideBeneficiaryGuardianOperationRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BeneficiaryUpdateBatchResponse>>> GetUpdateBatchesAsync(BeneficiaryUpdateBatchKind? kind = null, BeneficiaryOperationStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryUpdateBatchResponse>> CreateUpdateBatchAsync(CreateBeneficiaryUpdateBatchRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryUpdateBatchResponse>> UpdateBatchProgressAsync(int id, UpdateBeneficiaryBatchProgressRequest request, CancellationToken cancellationToken = default);
}
