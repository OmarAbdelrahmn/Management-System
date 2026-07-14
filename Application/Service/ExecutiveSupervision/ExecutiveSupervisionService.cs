using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.ExecutiveSupervision;
using Application.Service.TaskManagement;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.ExecutiveSupervision;

public class ExecutiveSupervisionService(ApplicationDbcontext dbcontext, ITaskManagementService? approvalWorkflow = null) : IExecutiveSupervisionService
{
    public async Task<Result<ExecutiveSupervisionDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var balanceEntries = await dbcontext.AidCommitteeCreditEntries.AsNoTracking().ToListAsync(cancellationToken);
        var balance = balanceEntries.Sum(x => x.EntryType == AidCommitteeCreditType.Expense ? -x.Amount : x.Amount);
        return Result.Success(new ExecutiveSupervisionDashboardResponse(
            await dbcontext.EstablishmentDocuments.CountAsync(cancellationToken),
            balance,
            await dbcontext.ExecutiveApprovalRequests.CountAsync(x => x.Status == ExecutiveWorkflowStatus.Pending, cancellationToken),
            await dbcontext.PaymentAuthorizations.CountAsync(x => x.Status == ExecutiveWorkflowStatus.Pending, cancellationToken),
            await dbcontext.AdministrativeDecisionRecords.CountAsync(cancellationToken)));
    }

    public async Task<Result<IEnumerable<EstablishmentDocumentResponse>>> GetFoundationDocumentsAsync(EstablishmentDocumentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.EstablishmentDocuments.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<EstablishmentDocumentResponse>>(await query.OrderBy(x => x.DocumentCode).Select(x => MapFoundation(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<EstablishmentDocumentResponse>> SaveFoundationDocumentAsync(int? id, SaveEstablishmentDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentCode) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<EstablishmentDocumentResponse>(ExecutiveSupervisionErrors.InvalidRequest);
        var code = request.DocumentCode.Trim();
        if (await dbcontext.EstablishmentDocuments.AnyAsync(x => x.DocumentCode == code && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<EstablishmentDocumentResponse>(ExecutiveSupervisionErrors.DuplicateFoundationDocument);
        var entity = id.HasValue ? await dbcontext.EstablishmentDocuments.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new EstablishmentDocument();
        if (entity is null) return Result.Failure<EstablishmentDocumentResponse>(ExecutiveSupervisionErrors.FoundationDocumentNotFound);
        if (!id.HasValue) dbcontext.EstablishmentDocuments.Add(entity);
        entity.DocumentCode = code;
        entity.Title = request.Title.Trim();
        entity.OwnerDepartment = TrimOrNull(request.OwnerDepartment);
        entity.FileUrl = TrimOrNull(request.FileUrl);
        entity.Status = request.Status;
        entity.HelperNotes = TrimOrNull(request.HelperNotes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapFoundation(entity));
    }

    public async Task<Result<IEnumerable<AidCommitteeCreditEntryResponse>>> GetAidCommitteeEntriesAsync(CancellationToken cancellationToken = default) =>
        Result.Success<IEnumerable<AidCommitteeCreditEntryResponse>>(await dbcontext.AidCommitteeCreditEntries.AsNoTracking().OrderByDescending(x => x.EntryDate).Select(x => MapAidCredit(x)).ToListAsync(cancellationToken));

    public async Task<Result<AidCommitteeCreditEntryResponse>> SaveAidCommitteeEntryAsync(int? id, SaveAidCommitteeCreditEntryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EntryNumber) || request.Amount < 0)
            return Result.Failure<AidCommitteeCreditEntryResponse>(ExecutiveSupervisionErrors.InvalidRequest);
        var number = request.EntryNumber.Trim();
        if (await dbcontext.AidCommitteeCreditEntries.AnyAsync(x => x.EntryNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<AidCommitteeCreditEntryResponse>(ExecutiveSupervisionErrors.DuplicateAidCommitteeEntry);
        var entity = id.HasValue ? await dbcontext.AidCommitteeCreditEntries.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new AidCommitteeCreditEntry();
        if (entity is null) return Result.Failure<AidCommitteeCreditEntryResponse>(ExecutiveSupervisionErrors.AidCommitteeEntryNotFound);
        if (!id.HasValue) dbcontext.AidCommitteeCreditEntries.Add(entity);
        entity.EntryNumber = number;
        entity.EntryType = request.EntryType;
        entity.Amount = request.Amount;
        entity.EntryDate = request.EntryDate;
        entity.Reference = TrimOrNull(request.Reference);
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAidCredit(entity));
    }

    public async Task<Result<IEnumerable<ExecutiveApprovalRequestResponse>>> GetApprovalsAsync(ExecutiveApprovalKind? kind = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.ExecutiveApprovalRequests.AsNoTracking().AsQueryable();
        if (kind.HasValue) query = query.Where(x => x.ApprovalKind == kind.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<ExecutiveApprovalRequestResponse>>(await query.OrderBy(x => x.Status).ThenByDescending(x => x.RequestedAt).Select(x => MapApproval(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<ExecutiveApprovalRequestResponse>> SaveApprovalAsync(int? id, SaveExecutiveApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestNumber) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.RequestedBy) || request.Amount < 0)
            return Result.Failure<ExecutiveApprovalRequestResponse>(ExecutiveSupervisionErrors.InvalidRequest);
        var number = request.RequestNumber.Trim();
        if (await dbcontext.ExecutiveApprovalRequests.AnyAsync(x => x.RequestNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<ExecutiveApprovalRequestResponse>(ExecutiveSupervisionErrors.DuplicateApprovalRequest);
        var entity = id.HasValue ? await dbcontext.ExecutiveApprovalRequests.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new ExecutiveApprovalRequest();
        if (entity is null) return Result.Failure<ExecutiveApprovalRequestResponse>(ExecutiveSupervisionErrors.ApprovalNotFound);
        if (!id.HasValue) dbcontext.ExecutiveApprovalRequests.Add(entity);
        entity.RequestNumber = number;
        entity.ApprovalKind = request.ApprovalKind;
        entity.Subject = request.Subject.Trim();
        entity.Amount = request.Amount;
        entity.RequestedBy = request.RequestedBy.Trim();
        entity.Status = request.Status;
        entity.DecisionNotes = TrimOrNull(request.DecisionNotes);
        entity.RequestedAt = request.RequestedAt;
        entity.DecidedAt = request.Status == ExecutiveWorkflowStatus.Pending ? null : DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        if (!id.HasValue && entity.Status == ExecutiveWorkflowStatus.Pending && approvalWorkflow is not null)
            await approvalWorkflow.EnsureApprovalRequestForEntityAsync(
                nameof(ExecutiveApprovalRequest), entity.Id, entity.Subject, cancellationToken: cancellationToken);
        return Result.Success(MapApproval(entity));
    }

    public async Task<Result> DecideApprovalAsync(int id, DecideExecutiveWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.ExecutiveApprovalRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure(ExecutiveSupervisionErrors.ApprovalNotFound);
        entity.Status = request.Status;
        entity.DecisionNotes = TrimOrNull(request.DecisionNotes) ?? entity.DecisionNotes;
        entity.DecidedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<PaymentAuthorizationResponse>>> GetPaymentAuthorizationsAsync(ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.PaymentAuthorizations.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<PaymentAuthorizationResponse>>(await query.OrderBy(x => x.Status).ThenByDescending(x => x.AuthorizationDate).Select(x => MapAuthorization(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<PaymentAuthorizationResponse>> SavePaymentAuthorizationAsync(int? id, SavePaymentAuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AuthorizationNumber) || string.IsNullOrWhiteSpace(request.PayeeName) || string.IsNullOrWhiteSpace(request.Purpose) || request.Amount < 0)
            return Result.Failure<PaymentAuthorizationResponse>(ExecutiveSupervisionErrors.InvalidRequest);
        var number = request.AuthorizationNumber.Trim();
        if (await dbcontext.PaymentAuthorizations.AnyAsync(x => x.AuthorizationNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<PaymentAuthorizationResponse>(ExecutiveSupervisionErrors.DuplicateAuthorization);
        var entity = id.HasValue ? await dbcontext.PaymentAuthorizations.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new PaymentAuthorization();
        if (entity is null) return Result.Failure<PaymentAuthorizationResponse>(ExecutiveSupervisionErrors.AuthorizationNotFound);
        if (!id.HasValue) dbcontext.PaymentAuthorizations.Add(entity);
        entity.AuthorizationNumber = number;
        entity.PayeeName = request.PayeeName.Trim();
        entity.Purpose = request.Purpose.Trim();
        entity.Amount = request.Amount;
        entity.Status = request.Status;
        entity.RejectionNote = TrimOrNull(request.RejectionNote);
        entity.AuthorizationDate = request.AuthorizationDate;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAuthorization(entity));
    }

    public async Task<Result> DecidePaymentAuthorizationAsync(int id, DecideExecutiveWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.PaymentAuthorizations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return Result.Failure(ExecutiveSupervisionErrors.AuthorizationNotFound);
        entity.Status = request.Status;
        entity.RejectionNote = TrimOrNull(request.DecisionNotes) ?? entity.RejectionNote;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<AdministrativeDecisionRecordResponse>>> GetAdministrativeDecisionsAsync(AdministrativeDecisionType? type = null, ExecutiveWorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.AdministrativeDecisionRecords.AsNoTracking().AsQueryable();
        if (type.HasValue) query = query.Where(x => x.DecisionType == type.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<AdministrativeDecisionRecordResponse>>(await query.OrderByDescending(x => x.DecisionDate).Select(x => MapDecision(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<AdministrativeDecisionRecordResponse>> SaveAdministrativeDecisionAsync(int? id, SaveAdministrativeDecisionRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DecisionNumber) || string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<AdministrativeDecisionRecordResponse>(ExecutiveSupervisionErrors.InvalidRequest);
        var number = request.DecisionNumber.Trim();
        if (await dbcontext.AdministrativeDecisionRecords.AnyAsync(x => x.DecisionNumber == number && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<AdministrativeDecisionRecordResponse>(ExecutiveSupervisionErrors.DuplicateDecision);
        var entity = id.HasValue ? await dbcontext.AdministrativeDecisionRecords.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken) : new AdministrativeDecisionRecord();
        if (entity is null) return Result.Failure<AdministrativeDecisionRecordResponse>(ExecutiveSupervisionErrors.DecisionNotFound);
        if (!id.HasValue) dbcontext.AdministrativeDecisionRecords.Add(entity);
        entity.DecisionNumber = number;
        entity.DecisionType = request.DecisionType;
        entity.Title = request.Title.Trim();
        entity.RelatedMeetingCode = TrimOrNull(request.RelatedMeetingCode);
        entity.AssignedTo = TrimOrNull(request.AssignedTo);
        entity.Status = request.Status;
        entity.DecisionDate = request.DecisionDate;
        entity.ExportTemplateName = TrimOrNull(request.ExportTemplateName);
        entity.Notes = TrimOrNull(request.Notes);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapDecision(entity));
    }

    private static EstablishmentDocumentResponse MapFoundation(EstablishmentDocument x) => new(x.Id, x.DocumentCode, x.Title, x.OwnerDepartment, x.FileUrl, x.Status.ToString(), x.HelperNotes);
    private static AidCommitteeCreditEntryResponse MapAidCredit(AidCommitteeCreditEntry x) => new(x.Id, x.EntryNumber, x.EntryType.ToString(), x.Amount, x.EntryDate, x.Reference, x.Notes);
    private static ExecutiveApprovalRequestResponse MapApproval(ExecutiveApprovalRequest x) => new(x.Id, x.RequestNumber, x.ApprovalKind.ToString(), x.Subject, x.Amount, x.RequestedBy, x.Status.ToString(), x.DecisionNotes, x.RequestedAt, x.DecidedAt);
    private static PaymentAuthorizationResponse MapAuthorization(PaymentAuthorization x) => new(x.Id, x.AuthorizationNumber, x.PayeeName, x.Purpose, x.Amount, x.Status.ToString(), x.RejectionNote, x.AuthorizationDate);
    private static AdministrativeDecisionRecordResponse MapDecision(AdministrativeDecisionRecord x) => new(x.Id, x.DecisionNumber, x.DecisionType.ToString(), x.Title, x.RelatedMeetingCode, x.AssignedTo, x.Status.ToString(), x.DecisionDate, x.ExportTemplateName, x.Notes);
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
