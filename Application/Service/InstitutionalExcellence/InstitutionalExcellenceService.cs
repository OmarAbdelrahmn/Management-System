using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.InstitutionalExcellence;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.InstitutionalExcellence;

public class InstitutionalExcellenceService(ApplicationDbcontext dbcontext) : IInstitutionalExcellenceService
{
    public async Task<Result<InstitutionalExcellenceDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var performanceMeasures = await dbcontext.PerformanceMeasures.AsNoTracking().ToListAsync(cancellationToken);
        var governanceCriteria = await dbcontext.GovernanceCriteria.AsNoTracking().ToListAsync(cancellationToken);
        var indicators = await dbcontext.StrategicIndicators.AsNoTracking().ToListAsync(cancellationToken);

        return Result.Success(new InstitutionalExcellenceDashboardResponse(
            performanceMeasures.Count,
            AverageAchievement(performanceMeasures.Select(x => Achievement(x.ActualValue, x.TargetValue))),
            await dbcontext.GovernanceCycles.CountAsync(x => x.IsActive, cancellationToken),
            GovernanceScore(governanceCriteria),
            await dbcontext.GovernanceTasks.CountAsync(x => x.Status == GovernanceTaskStatus.Pending || x.Status == GovernanceTaskStatus.InProgress || x.Status == GovernanceTaskStatus.Overdue, cancellationToken),
            await dbcontext.StrategicPlans.CountAsync(x => x.Status == ExcellenceRecordStatus.Active, cancellationToken),
            indicators.Count(x => x.Status == StrategicIndicatorStatus.Active),
            AverageAchievement(indicators.Select(x => Achievement(x.ActualValue, x.TargetValue)))));
    }

    public async Task<Result<IEnumerable<PerformanceMeasureResponse>>> GetPerformanceMeasuresAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.PerformanceMeasures.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<PerformanceMeasureResponse>>(await query.OrderBy(x => x.Code).Select(x => MapPerformance(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<PerformanceMeasureResponse>> SavePerformanceMeasureAsync(int? id, SavePerformanceMeasureRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title) || request.TargetValue < 0 || request.ActualValue < 0)
            return Result.Failure<PerformanceMeasureResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.PerformanceMeasures, id, cancellationToken);
        if (entity is null)
            return Result.Failure<PerformanceMeasureResponse>(InstitutionalExcellenceErrors.PerformanceMeasureNotFound);

        entity.Code = request.Code.Trim();
        entity.Title = request.Title.Trim();
        entity.MeasureType = request.MeasureType;
        entity.TargetValue = request.TargetValue;
        entity.ActualValue = request.ActualValue;
        entity.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "%" : request.Unit.Trim();
        entity.ReportingPeriod = string.IsNullOrWhiteSpace(request.ReportingPeriod) ? DateTime.UtcNow.AddHours(3).Year.ToString() : request.ReportingPeriod.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapPerformance(entity));
    }

    public async Task<Result<IEnumerable<GovernanceCycleResponse>>> GetGovernanceCyclesAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.GovernanceCycles.AsNoTracking().Include(x => x.Criteria).Include(x => x.Tasks).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<GovernanceCycleResponse>>(await query.OrderByDescending(x => x.Year).Select(x => MapCycle(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<GovernanceCycleResponse>> SaveGovernanceCycleAsync(int? id, SaveGovernanceCycleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Year < 2000)
            return Result.Failure<GovernanceCycleResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.GovernanceCycles, id, cancellationToken);
        if (entity is null)
            return Result.Failure<GovernanceCycleResponse>(InstitutionalExcellenceErrors.GovernanceCycleNotFound);

        if (request.IsActive)
        {
            var otherActiveCycles = await dbcontext.GovernanceCycles.Where(x => x.Id != entity.Id && x.IsActive).ToListAsync(cancellationToken);
            foreach (var cycle in otherActiveCycles)
                cycle.IsActive = false;
        }

        entity.Title = request.Title.Trim();
        entity.Year = request.Year;
        entity.IsActive = request.IsActive;
        entity.ActivatedAt = request.IsActive ? entity.ActivatedAt ?? DateTime.UtcNow.AddHours(3) : null;
        entity.Status = request.Status;
        entity.RoadmapNotes = request.RoadmapNotes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(entity).Collection(x => x.Criteria).LoadAsync(cancellationToken);
        await dbcontext.Entry(entity).Collection(x => x.Tasks).LoadAsync(cancellationToken);
        return Result.Success(MapCycle(entity));
    }

    public async Task<Result<IEnumerable<GovernanceCriterionResponse>>> GetGovernanceCriteriaAsync(int? cycleId = null, GovernanceCriterionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.GovernanceCriteria.AsNoTracking().Include(x => x.GovernanceCycle).Include(x => x.Attachments).AsQueryable();
        if (cycleId.HasValue) query = query.Where(x => x.GovernanceCycleId == cycleId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<GovernanceCriterionResponse>>(await query.OrderBy(x => x.Code).Select(x => MapCriterion(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<GovernanceCriterionResponse>> SaveGovernanceCriterionAsync(int? id, SaveGovernanceCriterionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title) || request.Weight < 0 || request.TargetScore < 0 || request.ActualScore < 0)
            return Result.Failure<GovernanceCriterionResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.GovernanceCycles.AnyAsync(x => x.Id == request.GovernanceCycleId, cancellationToken))
            return Result.Failure<GovernanceCriterionResponse>(InstitutionalExcellenceErrors.GovernanceCycleNotFound);

        var entity = await FindOrCreateAsync(dbcontext.GovernanceCriteria, id, cancellationToken);
        if (entity is null)
            return Result.Failure<GovernanceCriterionResponse>(InstitutionalExcellenceErrors.GovernanceCriterionNotFound);

        entity.GovernanceCycleId = request.GovernanceCycleId;
        entity.Code = request.Code.Trim();
        entity.Title = request.Title.Trim();
        entity.Weight = request.Weight;
        entity.TargetScore = request.TargetScore;
        entity.ActualScore = request.ActualScore;
        entity.Status = request.Status;
        entity.Answer = request.Answer?.Trim();
        entity.VerificationNotes = request.VerificationNotes?.Trim();
        entity.FinancialIndicatorValue = request.FinancialIndicatorValue;

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.GovernanceCriteria.AsNoTracking().Include(x => x.GovernanceCycle).Include(x => x.Attachments).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapCriterion(saved));
    }

    public async Task<Result<IEnumerable<GovernanceAttachmentResponse>>> GetGovernanceAttachmentsAsync(int? criterionId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.GovernanceAttachments.AsNoTracking().Include(x => x.GovernanceCriterion).AsQueryable();
        if (criterionId.HasValue) query = query.Where(x => x.GovernanceCriterionId == criterionId.Value);
        return Result.Success<IEnumerable<GovernanceAttachmentResponse>>(await query.OrderByDescending(x => x.UploadedAt).Select(x => MapAttachment(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<GovernanceAttachmentResponse>> SaveGovernanceAttachmentAsync(int? id, SaveGovernanceAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.FileUrl))
            return Result.Failure<GovernanceAttachmentResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.GovernanceCriteria.AnyAsync(x => x.Id == request.GovernanceCriterionId, cancellationToken))
            return Result.Failure<GovernanceAttachmentResponse>(InstitutionalExcellenceErrors.GovernanceCriterionNotFound);

        var entity = await FindOrCreateAsync(dbcontext.GovernanceAttachments, id, cancellationToken);
        if (entity is null)
            return Result.Failure<GovernanceAttachmentResponse>(InstitutionalExcellenceErrors.GovernanceAttachmentNotFound);

        entity.GovernanceCriterionId = request.GovernanceCriterionId;
        entity.FileName = request.FileName.Trim();
        entity.FileUrl = request.FileUrl.Trim();
        entity.Notes = request.Notes?.Trim();
        entity.UploadedAt = request.UploadedAt ?? DateTime.UtcNow.AddHours(3);

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.GovernanceAttachments.AsNoTracking().Include(x => x.GovernanceCriterion).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapAttachment(saved));
    }

    public async Task<Result<IEnumerable<GovernanceTaskResponse>>> GetGovernanceTasksAsync(int? cycleId = null, GovernanceTaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.GovernanceTasks.AsNoTracking().Include(x => x.GovernanceCycle).AsQueryable();
        if (cycleId.HasValue) query = query.Where(x => x.GovernanceCycleId == cycleId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<GovernanceTaskResponse>>(await query.OrderBy(x => x.DueDate).Select(x => MapTask(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<GovernanceTaskResponse>> SaveGovernanceTaskAsync(int? id, SaveGovernanceTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.ProgressPercent is < 0 or > 100)
            return Result.Failure<GovernanceTaskResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (request.GovernanceCycleId.HasValue && !await dbcontext.GovernanceCycles.AnyAsync(x => x.Id == request.GovernanceCycleId.Value, cancellationToken))
            return Result.Failure<GovernanceTaskResponse>(InstitutionalExcellenceErrors.GovernanceCycleNotFound);

        var entity = await FindOrCreateAsync(dbcontext.GovernanceTasks, id, cancellationToken);
        if (entity is null)
            return Result.Failure<GovernanceTaskResponse>(InstitutionalExcellenceErrors.GovernanceTaskNotFound);

        entity.GovernanceCycleId = request.GovernanceCycleId;
        entity.Title = request.Title.Trim();
        entity.OwnerName = request.OwnerName?.Trim();
        entity.DueDate = request.DueDate;
        entity.Status = request.Status;
        entity.ProgressPercent = request.ProgressPercent;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.GovernanceTasks.AsNoTracking().Include(x => x.GovernanceCycle).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapTask(saved));
    }

    public async Task<Result<GovernanceReportResponse>> GetGovernanceReportAsync(int? cycleId = null, CancellationToken cancellationToken = default)
    {
        var criteriaQuery = dbcontext.GovernanceCriteria.AsNoTracking().AsQueryable();
        var tasksQuery = dbcontext.GovernanceTasks.AsNoTracking().AsQueryable();
        if (cycleId.HasValue)
        {
            criteriaQuery = criteriaQuery.Where(x => x.GovernanceCycleId == cycleId.Value);
            tasksQuery = tasksQuery.Where(x => x.GovernanceCycleId == cycleId.Value);
        }

        var criteria = await criteriaQuery.ToListAsync(cancellationToken);
        var tasks = await tasksQuery.ToListAsync(cancellationToken);
        var criterionIds = criteria.Select(x => x.Id).ToList();
        var attachmentsCount = await dbcontext.GovernanceAttachments.CountAsync(x => criterionIds.Contains(x.GovernanceCriterionId), cancellationToken);

        return Result.Success(new GovernanceReportResponse(
            GovernanceScore(criteria),
            criteria.Count,
            criteria.Count(x => x.Status == GovernanceCriterionStatus.Met),
            attachmentsCount,
            tasks.Count(x => x.Status != GovernanceTaskStatus.Completed),
            tasks.Count(x => x.Status == GovernanceTaskStatus.Completed)));
    }

    public async Task<Result<IEnumerable<StrategicPlanResponse>>> GetStrategicPlansAsync(ExcellenceRecordStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.StrategicPlans.AsNoTracking().Include(x => x.Perspectives).Include(x => x.Indicators).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<StrategicPlanResponse>>(await query.OrderByDescending(x => x.StartDate).Select(x => MapPlan(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<StrategicPlanResponse>> SaveStrategicPlanAsync(int? id, SaveStrategicPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.EndDate < request.StartDate)
            return Result.Failure<StrategicPlanResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        var entity = await FindOrCreateAsync(dbcontext.StrategicPlans, id, cancellationToken);
        if (entity is null)
            return Result.Failure<StrategicPlanResponse>(InstitutionalExcellenceErrors.StrategicPlanNotFound);

        entity.Title = request.Title.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.Vision = request.Vision?.Trim();
        entity.Mission = request.Mission?.Trim();
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        await dbcontext.Entry(entity).Collection(x => x.Perspectives).LoadAsync(cancellationToken);
        await dbcontext.Entry(entity).Collection(x => x.Indicators).LoadAsync(cancellationToken);
        return Result.Success(MapPlan(entity));
    }

    public async Task<Result<IEnumerable<StrategicPerspectiveResponse>>> GetStrategicPerspectivesAsync(int? planId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.StrategicPerspectives.AsNoTracking().Include(x => x.StrategicPlan).Include(x => x.Goals).AsQueryable();
        if (planId.HasValue) query = query.Where(x => x.StrategicPlanId == planId.Value);
        return Result.Success<IEnumerable<StrategicPerspectiveResponse>>(await query.OrderBy(x => x.SortOrder).Select(x => MapPerspective(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<StrategicPerspectiveResponse>> SaveStrategicPerspectiveAsync(int? id, SaveStrategicPerspectiveRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<StrategicPerspectiveResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.StrategicPlans.AnyAsync(x => x.Id == request.StrategicPlanId, cancellationToken))
            return Result.Failure<StrategicPerspectiveResponse>(InstitutionalExcellenceErrors.StrategicPlanNotFound);

        var entity = await FindOrCreateAsync(dbcontext.StrategicPerspectives, id, cancellationToken);
        if (entity is null)
            return Result.Failure<StrategicPerspectiveResponse>(InstitutionalExcellenceErrors.StrategicPerspectiveNotFound);

        entity.StrategicPlanId = request.StrategicPlanId;
        entity.Name = request.Name.Trim();
        entity.SortOrder = request.SortOrder;

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.StrategicPerspectives.AsNoTracking().Include(x => x.StrategicPlan).Include(x => x.Goals).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapPerspective(saved));
    }

    public async Task<Result<IEnumerable<StrategicGoalResponse>>> GetStrategicGoalsAsync(int? perspectiveId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.StrategicGoals.AsNoTracking().Include(x => x.StrategicPerspective).AsQueryable();
        if (perspectiveId.HasValue) query = query.Where(x => x.StrategicPerspectiveId == perspectiveId.Value);
        return Result.Success<IEnumerable<StrategicGoalResponse>>(await query.OrderBy(x => x.SortOrder).Select(x => MapGoal(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<StrategicGoalResponse>> SaveStrategicGoalAsync(int? id, SaveStrategicGoalRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<StrategicGoalResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.StrategicPerspectives.AnyAsync(x => x.Id == request.StrategicPerspectiveId, cancellationToken))
            return Result.Failure<StrategicGoalResponse>(InstitutionalExcellenceErrors.StrategicPerspectiveNotFound);

        var entity = await FindOrCreateAsync(dbcontext.StrategicGoals, id, cancellationToken);
        if (entity is null)
            return Result.Failure<StrategicGoalResponse>(InstitutionalExcellenceErrors.StrategicGoalNotFound);

        entity.StrategicPerspectiveId = request.StrategicPerspectiveId;
        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim();
        entity.Vision2030Alignment = request.Vision2030Alignment?.Trim();
        entity.SustainabilityAlignment = request.SustainabilityAlignment?.Trim();
        entity.SortOrder = request.SortOrder;

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.StrategicGoals.AsNoTracking().Include(x => x.StrategicPerspective).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapGoal(saved));
    }

    public async Task<Result<IEnumerable<StrategicIndicatorResponse>>> GetStrategicIndicatorsAsync(int? planId = null, StrategicIndicatorKind? kind = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.StrategicIndicators.AsNoTracking().Include(x => x.StrategicPlan).Include(x => x.StrategicGoal).AsQueryable();
        if (planId.HasValue) query = query.Where(x => x.StrategicPlanId == planId.Value);
        if (kind.HasValue) query = query.Where(x => x.Kind == kind.Value);
        return Result.Success<IEnumerable<StrategicIndicatorResponse>>(await query.OrderBy(x => x.Kind).ThenBy(x => x.Name).Select(x => MapIndicator(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<StrategicIndicatorResponse>> SaveStrategicIndicatorAsync(int? id, SaveStrategicIndicatorRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.TargetValue < 0 || request.ActualValue < 0)
            return Result.Failure<StrategicIndicatorResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.StrategicPlans.AnyAsync(x => x.Id == request.StrategicPlanId, cancellationToken))
            return Result.Failure<StrategicIndicatorResponse>(InstitutionalExcellenceErrors.StrategicPlanNotFound);

        if (request.StrategicGoalId.HasValue && !await dbcontext.StrategicGoals.AnyAsync(x => x.Id == request.StrategicGoalId.Value, cancellationToken))
            return Result.Failure<StrategicIndicatorResponse>(InstitutionalExcellenceErrors.StrategicGoalNotFound);

        if (request.ParentIndicatorId.HasValue && !await dbcontext.StrategicIndicators.AnyAsync(x => x.Id == request.ParentIndicatorId.Value, cancellationToken))
            return Result.Failure<StrategicIndicatorResponse>(InstitutionalExcellenceErrors.StrategicIndicatorNotFound);

        var entity = await FindOrCreateAsync(dbcontext.StrategicIndicators, id, cancellationToken);
        if (entity is null)
            return Result.Failure<StrategicIndicatorResponse>(InstitutionalExcellenceErrors.StrategicIndicatorNotFound);

        entity.StrategicPlanId = request.StrategicPlanId;
        entity.StrategicGoalId = request.StrategicGoalId;
        entity.ParentIndicatorId = request.ParentIndicatorId;
        entity.Kind = request.Kind;
        entity.Name = request.Name.Trim();
        entity.TargetValue = request.TargetValue;
        entity.ActualValue = request.ActualValue;
        entity.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "%" : request.Unit.Trim();
        entity.OwnerName = request.OwnerName?.Trim();
        entity.RelatedProjectName = request.RelatedProjectName?.Trim();
        entity.RelatedProgramName = request.RelatedProgramName?.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.StrategicIndicators.AsNoTracking().Include(x => x.StrategicPlan).Include(x => x.StrategicGoal).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapIndicator(saved));
    }

    public async Task<Result<IEnumerable<StrategicVariableResponse>>> GetStrategicVariablesAsync(int? planId = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.StrategicVariables.AsNoTracking().Include(x => x.StrategicPlan).AsQueryable();
        if (planId.HasValue) query = query.Where(x => x.StrategicPlanId == planId.Value);
        return Result.Success<IEnumerable<StrategicVariableResponse>>(await query.OrderBy(x => x.Name).Select(x => MapVariable(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<StrategicVariableResponse>> SaveStrategicVariableAsync(int? id, SaveStrategicVariableRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<StrategicVariableResponse>(InstitutionalExcellenceErrors.InvalidRequest);

        if (!await dbcontext.StrategicPlans.AnyAsync(x => x.Id == request.StrategicPlanId, cancellationToken))
            return Result.Failure<StrategicVariableResponse>(InstitutionalExcellenceErrors.StrategicPlanNotFound);

        var entity = await FindOrCreateAsync(dbcontext.StrategicVariables, id, cancellationToken);
        if (entity is null)
            return Result.Failure<StrategicVariableResponse>(InstitutionalExcellenceErrors.StrategicVariableNotFound);

        entity.StrategicPlanId = request.StrategicPlanId;
        entity.Name = request.Name.Trim();
        entity.Value = request.Value;
        entity.Source = request.Source?.Trim();
        entity.IsAutomated = request.IsAutomated;
        entity.LastFetchedAt = request.IsAutomated ? entity.LastFetchedAt : null;

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.StrategicVariables.AsNoTracking().Include(x => x.StrategicPlan).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapVariable(saved));
    }

    public async Task<Result<IEnumerable<StrategicVariableResponse>>> FetchAutomatedStrategicVariablesAsync(int planId, CancellationToken cancellationToken = default)
    {
        if (!await dbcontext.StrategicPlans.AnyAsync(x => x.Id == planId, cancellationToken))
            return Result.Failure<IEnumerable<StrategicVariableResponse>>(InstitutionalExcellenceErrors.StrategicPlanNotFound);

        var now = DateTime.UtcNow.AddHours(3);
        var variables = await dbcontext.StrategicVariables.Where(x => x.StrategicPlanId == planId && x.IsAutomated).ToListAsync(cancellationToken);
        foreach (var variable in variables)
            variable.LastFetchedAt = now;
        await dbcontext.SaveChangesAsync(cancellationToken);

        var result = await GetStrategicVariablesAsync(planId, cancellationToken);
        return Result.Success(result.Value.Where(x => x.IsAutomated));
    }

    private static decimal Achievement(decimal actual, decimal target)
    {
        if (target <= 0) return actual > 0 ? 100 : 0;
        return Math.Round(actual / target * 100, 2);
    }

    private static decimal AverageAchievement(IEnumerable<decimal> achievements)
    {
        var values = achievements.ToList();
        return values.Count == 0 ? 0 : Math.Round(values.Average(), 2);
    }

    private static decimal GovernanceScore(IReadOnlyCollection<GovernanceCriterion> criteria)
    {
        if (criteria.Count == 0)
            return 0;

        var totalWeight = criteria.Sum(x => x.Weight);
        if (totalWeight <= 0)
            return AverageAchievement(criteria.Select(x => Achievement(x.ActualScore, x.TargetScore)));

        return Math.Round(criteria.Sum(x => Achievement(x.ActualScore, x.TargetScore) * x.Weight) / totalWeight, 2);
    }

    private static async Task<T?> FindOrCreateAsync<T>(DbSet<T> set, int? id, CancellationToken cancellationToken) where T : class, new()
    {
        if (!id.HasValue)
        {
            var created = new T();
            set.Add(created);
            return created;
        }

        return await set.FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id.Value, cancellationToken);
    }

    private static PerformanceMeasureResponse MapPerformance(PerformanceMeasure x) =>
        new(x.Id, x.Code, x.Title, x.MeasureType.ToString(), x.TargetValue, x.ActualValue, x.Unit, x.ReportingPeriod, x.Status.ToString(), Achievement(x.ActualValue, x.TargetValue), x.Notes);

    private static GovernanceCycleResponse MapCycle(GovernanceCycle x) =>
        new(x.Id, x.Title, x.Year, x.IsActive, x.ActivatedAt, x.Status.ToString(), x.RoadmapNotes, GovernanceScore(x.Criteria.ToList()), x.Criteria.Count, x.Tasks.Count);

    private static GovernanceCriterionResponse MapCriterion(GovernanceCriterion x) =>
        new(x.Id, x.GovernanceCycleId, x.GovernanceCycle?.Title ?? string.Empty, x.Code, x.Title, x.Weight, x.TargetScore, x.ActualScore, x.Status.ToString(), x.Answer, x.VerificationNotes, x.FinancialIndicatorValue, x.Attachments.Count);

    private static GovernanceAttachmentResponse MapAttachment(GovernanceAttachment x) =>
        new(x.Id, x.GovernanceCriterionId, x.GovernanceCriterion?.Title ?? string.Empty, x.FileName, x.FileUrl, x.Notes, x.UploadedAt);

    private static GovernanceTaskResponse MapTask(GovernanceTask x) =>
        new(x.Id, x.GovernanceCycleId, x.GovernanceCycle?.Title ?? string.Empty, x.Title, x.OwnerName, x.DueDate, x.Status.ToString(), x.ProgressPercent, x.Notes);

    private static StrategicPlanResponse MapPlan(StrategicPlan x) =>
        new(x.Id, x.Title, x.StartDate, x.EndDate, x.Status.ToString(), x.Vision, x.Mission, x.Notes, x.Perspectives.Count, x.Indicators.Count, AverageAchievement(x.Indicators.Select(i => Achievement(i.ActualValue, i.TargetValue))));

    private static StrategicPerspectiveResponse MapPerspective(StrategicPerspective x) =>
        new(x.Id, x.StrategicPlanId, x.StrategicPlan?.Title ?? string.Empty, x.Name, x.SortOrder, x.Goals.Count);

    private static StrategicGoalResponse MapGoal(StrategicGoal x) =>
        new(x.Id, x.StrategicPerspectiveId, x.StrategicPerspective?.Name ?? string.Empty, x.Title, x.Description, x.Vision2030Alignment, x.SustainabilityAlignment, x.SortOrder);

    private static StrategicIndicatorResponse MapIndicator(StrategicIndicator x) =>
        new(x.Id, x.StrategicPlanId, x.StrategicPlan?.Title ?? string.Empty, x.StrategicGoalId, x.StrategicGoal?.Title ?? string.Empty, x.ParentIndicatorId, x.Kind.ToString(), x.Name, x.TargetValue, x.ActualValue, x.Unit, x.OwnerName, x.RelatedProjectName, x.RelatedProgramName, x.Status.ToString(), Achievement(x.ActualValue, x.TargetValue), x.Notes);

    private static StrategicVariableResponse MapVariable(StrategicVariable x) =>
        new(x.Id, x.StrategicPlanId, x.StrategicPlan?.Title ?? string.Empty, x.Name, x.Value, x.Source, x.IsAutomated, x.LastFetchedAt);
}
