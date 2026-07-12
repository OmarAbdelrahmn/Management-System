using Application.Abstraction;
using Application.Contracts.DocumentationArchive;
using Domain.Entities;

namespace Application.Service.DocumentationArchive;

public interface IDocumentationArchiveService
{
    Task<Result<DocumentationArchiveDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ArchiveDocumentResponse>>> GetArchiveDocumentsAsync(ArchiveDocumentCategory? category = null, ArchiveDocumentStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<ArchiveDocumentResponse>> SaveArchiveDocumentAsync(int? id, SaveArchiveDocumentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CorrespondenceRecordResponse>>> GetCorrespondenceAsync(CorrespondenceDirection? direction = null, CorrespondenceStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<CorrespondenceRecordResponse>> SaveCorrespondenceAsync(int? id, SaveCorrespondenceRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateCorrespondenceStatusAsync(int id, UpdateCorrespondenceStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<CorrespondenceRecordResponse>> RequestCorrespondenceRemovalAsync(int id, RequestCorrespondenceRemovalRequest request, CancellationToken cancellationToken = default);
    Task<Result<CorrespondenceRecordResponse>> DecideCorrespondenceRemovalAsync(int id, DecideCorrespondenceRemovalRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CorrespondenceOperationResponse>>> GetOperationsAsync(CorrespondenceOperationStatus? status = null, int? correspondenceRecordId = null, CancellationToken cancellationToken = default);
    Task<Result<CorrespondenceOperationResponse>> SaveOperationAsync(int? id, SaveCorrespondenceOperationRequest request, CancellationToken cancellationToken = default);
    Task<Result> CompleteOperationAsync(int id, CompleteCorrespondenceOperationRequest request, CancellationToken cancellationToken = default);
}
