using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.DocumentationArchive;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.DocumentationArchive;

public class DocumentationArchiveService(ApplicationDbcontext dbcontext) : IDocumentationArchiveService
{
    public async Task<Result<DocumentationArchiveDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        Result.Success(new DocumentationArchiveDashboardResponse(
            await dbcontext.ArchiveDocuments.CountAsync(cancellationToken),
            await dbcontext.ArchiveDocuments.CountAsync(x => x.Status == ArchiveDocumentStatus.Active, cancellationToken),
            await dbcontext.CorrespondenceRecords.CountAsync(x => x.Direction == CorrespondenceDirection.Incoming, cancellationToken),
            await dbcontext.CorrespondenceRecords.CountAsync(x => x.Direction == CorrespondenceDirection.Outgoing, cancellationToken),
            await dbcontext.CorrespondenceOperations.CountAsync(x => x.Status == CorrespondenceOperationStatus.Open, cancellationToken),
            await dbcontext.CorrespondenceOperations.CountAsync(x => x.Status == CorrespondenceOperationStatus.Completed, cancellationToken)));

    public async Task<Result<IEnumerable<ArchiveDocumentResponse>>> GetArchiveDocumentsAsync(ArchiveDocumentCategory? category = null, ArchiveDocumentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ArchiveDocuments.AsNoTracking().AsQueryable();
        if (category.HasValue) query = query.Where(x => x.Category == category.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<ArchiveDocumentResponse>>(await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.DocumentNumber)
            .Select(x => MapArchiveDocument(x))
            .ToListAsync(cancellationToken));
    }

    public async Task<Result<ArchiveDocumentResponse>> SaveArchiveDocumentAsync(int? id, SaveArchiveDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentNumber) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<ArchiveDocumentResponse>(DocumentationArchiveErrors.InvalidRequest);

        var documentNumber = request.DocumentNumber.Trim();
        if (await dbcontext.ArchiveDocuments.AnyAsync(x => x.DocumentNumber == documentNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ArchiveDocumentResponse>(DocumentationArchiveErrors.DuplicateDocumentNumber);

        var entity = id.HasValue
            ? await dbcontext.ArchiveDocuments.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new ArchiveDocument();
        if (entity is null)
            return Result.Failure<ArchiveDocumentResponse>(DocumentationArchiveErrors.ArchiveDocumentNotFound);
        if (!id.HasValue) dbcontext.ArchiveDocuments.Add(entity);

        entity.DocumentNumber = documentNumber;
        entity.Title = request.Title.Trim();
        entity.Category = request.Category;
        entity.FileUrl = TrimOrNull(request.FileUrl);
        entity.OwnerDepartment = TrimOrNull(request.OwnerDepartment);
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapArchiveDocument(entity));
    }

    public async Task<Result<IEnumerable<CorrespondenceRecordResponse>>> GetCorrespondenceAsync(CorrespondenceDirection? direction = null, CorrespondenceStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CorrespondenceRecords.AsNoTracking().Include(x => x.Operations).AsQueryable();
        if (direction.HasValue) query = query.Where(x => x.Direction == direction.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return Result.Success<IEnumerable<CorrespondenceRecordResponse>>(await query
            .OrderByDescending(x => x.MailDate)
            .ThenBy(x => x.MailNumber)
            .Select(x => MapCorrespondence(x))
            .ToListAsync(cancellationToken));
    }

    public async Task<Result<CorrespondenceRecordResponse>> SaveCorrespondenceAsync(int? id, SaveCorrespondenceRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MailNumber) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.PartyName))
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.InvalidRequest);

        var mailNumber = request.MailNumber.Trim();
        if (await dbcontext.CorrespondenceRecords.AnyAsync(x => x.MailNumber == mailNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.DuplicateMailNumber);

        var entity = id.HasValue
            ? await dbcontext.CorrespondenceRecords.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new CorrespondenceRecord();
        if (entity is null)
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.CorrespondenceNotFound);
        if (!id.HasValue) dbcontext.CorrespondenceRecords.Add(entity);

        entity.MailNumber = mailNumber;
        entity.Direction = request.Direction;
        entity.Subject = request.Subject.Trim();
        entity.PartyName = request.PartyName.Trim();
        entity.MailDate = request.MailDate;
        entity.BarcodeValue = TrimOrNull(request.BarcodeValue);
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCorrespondence(entity));
    }

    public async Task<Result> UpdateCorrespondenceStatusAsync(int id, UpdateCorrespondenceStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.CorrespondenceRecords.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure(DocumentationArchiveErrors.CorrespondenceNotFound);
        if (request.Status == CorrespondenceStatus.PendingRemovalApproval && HasOpenOperations(entity))
            return Result.Failure(DocumentationArchiveErrors.CorrespondenceHasOpenOperations);
        if (request.Status == CorrespondenceStatus.Removed)
        {
            if (entity.Status != CorrespondenceStatus.PendingRemovalApproval)
                return Result.Failure(DocumentationArchiveErrors.InvalidCorrespondenceStatusTransition);
            if (HasOpenOperations(entity))
                return Result.Failure(DocumentationArchiveErrors.CorrespondenceHasOpenOperations);
        }

        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes) ?? entity.Notes;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CorrespondenceRecordResponse>> RequestCorrespondenceRemovalAsync(int id, RequestCorrespondenceRemovalRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.CorrespondenceRecords.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.CorrespondenceNotFound);
        if (entity.Status == CorrespondenceStatus.Removed)
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.InvalidCorrespondenceStatusTransition);
        if (HasOpenOperations(entity))
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.CorrespondenceHasOpenOperations);

        entity.Status = CorrespondenceStatus.PendingRemovalApproval;
        entity.Notes = AppendNote(entity.Notes, request.Notes, 1000);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapCorrespondence(entity));
    }

    public async Task<Result<CorrespondenceRecordResponse>> DecideCorrespondenceRemovalAsync(int id, DecideCorrespondenceRemovalRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.CorrespondenceRecords.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.CorrespondenceNotFound);
        if (entity.Status != CorrespondenceStatus.PendingRemovalApproval)
            return Result.Failure<CorrespondenceRecordResponse>(DocumentationArchiveErrors.InvalidCorrespondenceStatusTransition);

        entity.Status = request.Approved ? CorrespondenceStatus.Removed : CorrespondenceStatus.Registered;
        entity.Notes = AppendNote(entity.Notes, request.Notes, 1000);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapCorrespondence(entity));
    }

    public async Task<Result<IEnumerable<CorrespondenceOperationResponse>>> GetOperationsAsync(CorrespondenceOperationStatus? status = null, int? correspondenceRecordId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.CorrespondenceOperations.AsNoTracking().Include(x => x.CorrespondenceRecord).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (correspondenceRecordId.HasValue) query = query.Where(x => x.CorrespondenceRecordId == correspondenceRecordId.Value);

        return Result.Success<IEnumerable<CorrespondenceOperationResponse>>(await query
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .ThenBy(x => x.OperationNumber)
            .Select(x => MapOperation(x))
            .ToListAsync(cancellationToken));
    }

    public async Task<Result<CorrespondenceOperationResponse>> SaveOperationAsync(int? id, SaveCorrespondenceOperationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CorrespondenceRecordId <= 0 || string.IsNullOrWhiteSpace(request.OperationNumber) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CorrespondenceOperationResponse>(DocumentationArchiveErrors.InvalidRequest);

        var correspondence = await dbcontext.CorrespondenceRecords
            .Include(x => x.Operations)
            .FirstOrDefaultAsync(x => x.Id == request.CorrespondenceRecordId, cancellationToken);
        if (correspondence is null)
            return Result.Failure<CorrespondenceOperationResponse>(DocumentationArchiveErrors.CorrespondenceNotFound);
        if (correspondence.Status is CorrespondenceStatus.PendingRemovalApproval or CorrespondenceStatus.Removed)
            return Result.Failure<CorrespondenceOperationResponse>(DocumentationArchiveErrors.InvalidCorrespondenceStatusTransition);

        var operationNumber = request.OperationNumber.Trim();
        if (await dbcontext.CorrespondenceOperations.AnyAsync(x => x.OperationNumber == operationNumber && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<CorrespondenceOperationResponse>(DocumentationArchiveErrors.DuplicateOperationNumber);

        var entity = id.HasValue
            ? await dbcontext.CorrespondenceOperations.Include(x => x.CorrespondenceRecord).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new CorrespondenceOperation();
        if (entity is null)
            return Result.Failure<CorrespondenceOperationResponse>(DocumentationArchiveErrors.OperationNotFound);
        if (!id.HasValue) dbcontext.CorrespondenceOperations.Add(entity);

        entity.CorrespondenceRecordId = request.CorrespondenceRecordId;
        entity.CorrespondenceRecord = correspondence;
        entity.OperationNumber = operationNumber;
        entity.Title = request.Title.Trim();
        entity.AssignedTo = TrimOrNull(request.AssignedTo);
        entity.DueDate = request.DueDate;
        entity.Status = request.Status;
        entity.Notes = TrimOrNull(request.Notes);
        entity.CompletedAt = request.Status == CorrespondenceOperationStatus.Completed
            ? DateTime.UtcNow.AddHours(3)
            : null;
        ApplyCorrespondenceOperationState(correspondence, entity);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapOperation(entity));
    }

    public async Task<Result> CompleteOperationAsync(int id, CompleteCorrespondenceOperationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.CorrespondenceOperations
            .Include(x => x.CorrespondenceRecord)
            .ThenInclude(x => x!.Operations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure(DocumentationArchiveErrors.OperationNotFound);

        entity.Status = CorrespondenceOperationStatus.Completed;
        entity.CompletedAt = request.CompletedAt;
        entity.Notes = TrimOrNull(request.Notes) ?? entity.Notes;
        if (entity.CorrespondenceRecord is not null)
            ApplyCorrespondenceOperationState(entity.CorrespondenceRecord, entity);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static void ApplyCorrespondenceOperationState(CorrespondenceRecord correspondence, CorrespondenceOperation currentOperation)
    {
        if (correspondence.Status is CorrespondenceStatus.Removed or CorrespondenceStatus.PendingRemovalApproval)
            return;

        var operations = correspondence.Operations
            .Where(x => x.Id != currentOperation.Id)
            .Append(currentOperation)
            .ToList();

        if (operations.Any(x => x.Status == CorrespondenceOperationStatus.Open))
        {
            correspondence.Status = CorrespondenceStatus.NeedsReply;
            return;
        }

        if (operations.Count > 0 && operations.All(x => x.Status != CorrespondenceOperationStatus.Open))
        {
            correspondence.Status = correspondence.Direction == CorrespondenceDirection.Incoming
                ? CorrespondenceStatus.Replied
                : CorrespondenceStatus.Completed;
        }
    }

    private static ArchiveDocumentResponse MapArchiveDocument(ArchiveDocument x) =>
        new(x.Id, x.DocumentNumber, x.Title, x.Category.ToString(), x.FileUrl, x.OwnerDepartment, x.Status.ToString(), x.Notes, x.CreatedAt);

    private static CorrespondenceRecordResponse MapCorrespondence(CorrespondenceRecord x) =>
        new(x.Id, x.MailNumber, x.Direction.ToString(), x.Subject, x.PartyName, x.MailDate, x.BarcodeValue, x.Status.ToString(), x.Notes, x.Operations.Count);

    private static CorrespondenceOperationResponse MapOperation(CorrespondenceOperation x) =>
        new(x.Id, x.CorrespondenceRecordId, x.CorrespondenceRecord?.MailNumber ?? string.Empty, x.OperationNumber, x.Title, x.AssignedTo, x.DueDate, x.CompletedAt, x.Status.ToString(), x.Notes);

    private static bool HasOpenOperations(CorrespondenceRecord correspondence) =>
        correspondence.Operations.Any(x => x.Status == CorrespondenceOperationStatus.Open);

    private static string? AppendNote(string? existingNotes, string? newNote, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(newNote))
            return existingNotes;

        var combined = string.IsNullOrWhiteSpace(existingNotes)
            ? newNote.Trim()
            : $"{existingNotes.Trim()}{Environment.NewLine}{newNote.Trim()}";
        return combined.Length <= maxLength ? combined : combined[..maxLength];
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
