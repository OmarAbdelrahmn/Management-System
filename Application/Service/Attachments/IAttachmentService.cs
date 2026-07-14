using Application.Abstraction;
using Application.Contracts.Attachments;

namespace Application.Service.Attachments;

public interface IAttachmentService
{
    Task<Result<AttachmentAccessResponse>> GetEntityAccessAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<Result<AttachmentResponse>> UploadAsync(AttachmentUploadInput input, CancellationToken cancellationToken = default);
    Task<Result<AttachmentResponse>> AddVersionAsync(int id, AttachmentUploadInput input, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AttachmentResponse>>> GetForEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AttachmentVersionResponse>>> GetVersionsAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<AttachmentDownloadResponse>> GetDownloadAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> UnlinkAsync(int linkId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}

public interface IAttachmentStorage
{
    Task<string> SaveTemporaryAsync(Stream content, CancellationToken cancellationToken = default);
    Task PromoteAsync(string temporaryPath, string permanentPath, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}

public enum AttachmentMalwareScanResult { Clean, Infected, Unavailable, Failed }

public interface IAttachmentMalwareScanner
{
    Task<AttachmentMalwareScanResult> ScanAsync(string temporaryPath, CancellationToken cancellationToken = default);
}
