using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.EvaluationFollowUp;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.EvaluationFollowUp;

public class EvaluationFollowUpService(ApplicationDbcontext dbcontext) : IEvaluationFollowUpService
{
    public async Task<Result<EvaluationFollowUpDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return Result.Success(new EvaluationFollowUpDashboardResponse(
            await dbcontext.FollowUpCases.CountAsync(x => x.Status == FollowUpCaseStatus.Requested, cancellationToken),
            await dbcontext.FollowUpCases.CountAsync(x => x.Status == FollowUpCaseStatus.Running, cancellationToken),
            await dbcontext.FollowUpCases.CountAsync(x => x.Status == FollowUpCaseStatus.PendingApproval, cancellationToken),
            await dbcontext.FollowUpCases.CountAsync(x => x.Status == FollowUpCaseStatus.Rejected, cancellationToken),
            await dbcontext.FollowUpCases.CountAsync(x => x.Status == FollowUpCaseStatus.Completed || x.Status == FollowUpCaseStatus.Approved, cancellationToken),
            await dbcontext.FollowUpActivities.CountAsync(cancellationToken),
            await dbcontext.FollowUpActivities.CountAsync(x => x.RequiresNextAction, cancellationToken)));
    }

    public async Task<Result<IEnumerable<FollowUpCaseResponse>>> GetCasesAsync(FollowUpCaseStatus? status = null, FollowUpSubjectType? subjectType = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FollowUpCases.AsNoTracking().Include(x => x.Activities).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (subjectType.HasValue) query = query.Where(x => x.SubjectType == subjectType.Value);
        return Result.Success<IEnumerable<FollowUpCaseResponse>>(await query.OrderByDescending(x => x.RequestDate).Select(x => MapCase(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FollowUpCaseResponse>> SaveCaseAsync(int? id, SaveFollowUpCaseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SubjectName) || string.IsNullOrWhiteSpace(request.RequestedBy))
            return Result.Failure<FollowUpCaseResponse>(EvaluationFollowUpErrors.InvalidRequest);

        FollowUpCase? entity;
        if (id.HasValue)
        {
            entity = await dbcontext.FollowUpCases.Include(x => x.Activities).FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (entity is null) return Result.Failure<FollowUpCaseResponse>(EvaluationFollowUpErrors.CaseNotFound);
        }
        else
        {
            entity = new FollowUpCase { CaseNumber = await GenerateCaseNumberAsync(request.RequestDate, cancellationToken) };
            dbcontext.FollowUpCases.Add(entity);
        }

        entity.SubjectType = request.SubjectType;
        entity.SubjectName = request.SubjectName.Trim();
        entity.ReferenceNumber = request.ReferenceNumber?.Trim();
        entity.RequestedBy = request.RequestedBy.Trim();
        entity.RequestDate = request.RequestDate;
        entity.DueDate = request.DueDate;
        entity.Priority = request.Priority;
        entity.Status = request.Status;
        entity.RejectionNote = request.RejectionNote?.Trim();
        entity.CompletionSummary = request.CompletionSummary?.Trim();
        entity.CompletedAt = request.Status is FollowUpCaseStatus.Completed or FollowUpCaseStatus.Approved ? DateTime.UtcNow.AddHours(3) : entity.CompletedAt;
        entity.ApprovalNote = request.ApprovalNote?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCase(entity));
    }

    public async Task<Result<FollowUpCaseResponse>> UpdateCaseStatusAsync(int id, UpdateFollowUpCaseStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.FollowUpCases.Include(x => x.Activities).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<FollowUpCaseResponse>(EvaluationFollowUpErrors.CaseNotFound);

        entity.Status = request.Status;
        if (request.Status == FollowUpCaseStatus.Rejected) entity.RejectionNote = request.Note?.Trim();
        if (request.Status is FollowUpCaseStatus.Completed or FollowUpCaseStatus.PendingApproval) entity.CompletionSummary = request.Note?.Trim();
        if (request.Status == FollowUpCaseStatus.Approved) entity.ApprovalNote = request.Note?.Trim();
        if (request.Status is FollowUpCaseStatus.Completed or FollowUpCaseStatus.Approved) entity.CompletedAt = DateTime.UtcNow.AddHours(3);

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapCase(entity));
    }

    public async Task<Result<IEnumerable<FollowUpActivityResponse>>> GetActivitiesAsync(int? caseId = null, FollowUpSubjectType? subjectType = null, bool nextActionsOnly = false, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FollowUpActivities.AsNoTracking().Include(x => x.FollowUpCase).AsQueryable();
        if (caseId.HasValue) query = query.Where(x => x.FollowUpCaseId == caseId.Value);
        if (subjectType.HasValue) query = query.Where(x => x.SubjectType == subjectType.Value);
        if (nextActionsOnly) query = query.Where(x => x.RequiresNextAction);
        return Result.Success<IEnumerable<FollowUpActivityResponse>>(await query.OrderByDescending(x => x.ActivityDate).Select(x => MapActivity(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FollowUpActivityResponse>> SaveActivityAsync(int? id, SaveFollowUpActivityRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SubjectName) || string.IsNullOrWhiteSpace(request.ActivityType) || string.IsNullOrWhiteSpace(request.Summary))
            return Result.Failure<FollowUpActivityResponse>(EvaluationFollowUpErrors.InvalidRequest);

        if (request.FollowUpCaseId.HasValue && !await dbcontext.FollowUpCases.AnyAsync(x => x.Id == request.FollowUpCaseId.Value, cancellationToken))
            return Result.Failure<FollowUpActivityResponse>(EvaluationFollowUpErrors.CaseNotFound);

        FollowUpActivity? entity;
        if (id.HasValue)
        {
            entity = await dbcontext.FollowUpActivities.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
            if (entity is null) return Result.Failure<FollowUpActivityResponse>(EvaluationFollowUpErrors.ActivityNotFound);
        }
        else
        {
            entity = new FollowUpActivity();
            dbcontext.FollowUpActivities.Add(entity);
        }

        entity.FollowUpCaseId = request.FollowUpCaseId;
        entity.SubjectType = request.SubjectType;
        entity.SubjectName = request.SubjectName.Trim();
        entity.ReferenceNumber = request.ReferenceNumber?.Trim();
        entity.ActivityDate = request.ActivityDate;
        entity.ActivityType = request.ActivityType.Trim();
        entity.Summary = request.Summary.Trim();
        entity.Result = request.Result?.Trim();
        entity.OwnerName = request.OwnerName?.Trim();
        entity.RequiresNextAction = request.RequiresNextAction;
        entity.NextActionDate = request.NextActionDate;

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.FollowUpActivities.AsNoTracking().Include(x => x.FollowUpCase).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapActivity(saved));
    }

    private async Task<string> GenerateCaseNumberAsync(DateTime requestDate, CancellationToken cancellationToken)
    {
        var prefix = $"FUP-{requestDate:yyyy}-";
        var count = await dbcontext.FollowUpCases.CountAsync(x => x.CaseNumber.StartsWith(prefix), cancellationToken) + 1;
        return $"{prefix}{count:0000}";
    }

    private static FollowUpCaseResponse MapCase(FollowUpCase x) =>
        new(x.Id, x.CaseNumber, x.SubjectType.ToString(), x.SubjectName, x.ReferenceNumber, x.RequestedBy, x.RequestDate, x.DueDate, x.Priority.ToString(), x.Status.ToString(), x.RejectionNote, x.CompletionSummary, x.CompletedAt, x.ApprovalNote, x.Activities.Count);

    private static FollowUpActivityResponse MapActivity(FollowUpActivity x) =>
        new(x.Id, x.FollowUpCaseId, x.FollowUpCase?.CaseNumber ?? string.Empty, x.SubjectType.ToString(), x.SubjectName, x.ReferenceNumber, x.ActivityDate, x.ActivityType, x.Summary, x.Result, x.OwnerName, x.RequiresNextAction, x.NextActionDate);
}
