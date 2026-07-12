using Domain.Entities;

namespace Application.Contracts.DocumentationArchive;

public record DocumentationArchiveDashboardResponse(int ArchiveDocumentsCount, int ActiveDocumentsCount, int IncomingCount, int OutgoingCount, int OpenOperationsCount, int CompletedOperationsCount);

public record ArchiveDocumentResponse(int Id, string DocumentNumber, string Title, string Category, string? FileUrl, string? OwnerDepartment, string Status, string? Notes, DateTime CreatedAt);
public record SaveArchiveDocumentRequest(string DocumentNumber, string Title, ArchiveDocumentCategory Category, string? FileUrl, string? OwnerDepartment, ArchiveDocumentStatus Status, string? Notes);

public record CorrespondenceRecordResponse(int Id, string MailNumber, string Direction, string Subject, string PartyName, DateTime MailDate, string? BarcodeValue, string Status, string? Notes, int OperationsCount);
public record SaveCorrespondenceRecordRequest(string MailNumber, CorrespondenceDirection Direction, string Subject, string PartyName, DateTime MailDate, string? BarcodeValue, CorrespondenceStatus Status, string? Notes);
public record UpdateCorrespondenceStatusRequest(CorrespondenceStatus Status, string? Notes);
public record RequestCorrespondenceRemovalRequest(string? Notes);
public record DecideCorrespondenceRemovalRequest(bool Approved, string? Notes);

public record CorrespondenceOperationResponse(int Id, int CorrespondenceRecordId, string MailNumber, string OperationNumber, string Title, string? AssignedTo, DateTime? DueDate, DateTime? CompletedAt, string Status, string? Notes);
public record SaveCorrespondenceOperationRequest(int CorrespondenceRecordId, string OperationNumber, string Title, string? AssignedTo, DateTime? DueDate, CorrespondenceOperationStatus Status, string? Notes);
public record CompleteCorrespondenceOperationRequest(DateTime CompletedAt, string? Notes);
