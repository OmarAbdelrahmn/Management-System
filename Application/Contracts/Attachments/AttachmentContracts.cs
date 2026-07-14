namespace Application.Contracts.Attachments;

public record AttachmentResponse(int Id, string OriginalFileName, string ContentType, long SizeBytes, string Category, int CurrentVersion, string Sha256, string ScanStatus, DateTime CreatedAt, IReadOnlyList<AttachmentLinkResponse> Links);
public record AttachmentLinkResponse(int Id, string EntityType, string EntityId, string? Label, DateTime CreatedAt);
public record AttachmentVersionResponse(int Id, int VersionNumber, string FileName, string ContentType, long SizeBytes, string Sha256, string ScanStatus, DateTime? ScannedAt, DateTime CreatedAt);
public record AttachmentUploadInput(Stream Content, string OriginalFileName, string ContentType, long SizeBytes, string Category, string? EntityType, string? EntityId, string? Label);
public record AttachmentDownloadResponse(string Path, string ContentType, string DownloadName);
public record AttachmentAccessResponse(bool CanView, bool CanManage);
