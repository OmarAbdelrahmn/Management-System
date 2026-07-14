using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.TaskManagement;
using Domain;
using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.TaskManagement;

public class TaskManagementService(ApplicationDbcontext dbcontext, ICurrentUserContext currentUserContext) : ITaskManagementService
{
    public async Task<Result<IEnumerable<UserPickerResponse>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbcontext.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new UserPickerResponse(x.Id, x.FullName, x.Email))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<UserPickerResponse>>(users);
    }

    public async Task<Result<IEnumerable<ManagementTaskResponse>>> SearchTasksAsync(TaskSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = BaseTaskQuery().AsQueryable();

        if (!request.IncludeDeleted)
            query = query.Where(x => x.Status != ManagementTaskStatus.Deleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.AssigneeUserId))
            query = query.Where(x => x.AssigneeUserId == request.AssigneeUserId);

        var tasks = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ManagementTaskResponse>>(tasks.Select(MapTask));
    }

    public async Task<Result<IEnumerable<ManagementTaskResponse>>> GetMyTasksAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tasks = await BaseTaskQuery()
            .Where(x => x.AssigneeUserId == userId && x.Status != ManagementTaskStatus.Deleted)
            .OrderBy(x => x.DueAt ?? DateTime.MaxValue)
            .ThenByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ManagementTaskResponse>>(tasks.Select(MapTask));
    }

    public async Task<Result<ManagementTaskResponse>> GetTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await BaseTaskQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return task is null
            ? Result.Failure<ManagementTaskResponse>(TaskManagementErrors.TaskNotFound)
            : Result.Success(MapTask(task));
    }

    public async Task<Result<ManagementTaskResponse>> CreateTaskAsync(CreateManagementTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.AssigneeUserId))
            return Result.Failure<ManagementTaskResponse>(TaskManagementErrors.InvalidRequest);

        var creatorUserId = currentUserContext.UserId ?? request.AssigneeUserId;
        var creatorExists = await dbcontext.Users.AnyAsync(x => x.Id == creatorUserId, cancellationToken);
        var assigneeExists = await dbcontext.Users.AnyAsync(x => x.Id == request.AssigneeUserId, cancellationToken);
        if (!creatorExists || !assigneeExists)
            return Result.Failure<ManagementTaskResponse>(TaskManagementErrors.UserNotFound);

        var task = new ManagementTask
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CreatorUserId = creatorUserId,
            AssigneeUserId = request.AssigneeUserId,
            DueAt = request.DueAt,
            Priority = request.Priority,
            RelatedEntityType = request.RelatedEntityType?.Trim(),
            RelatedEntityId = request.RelatedEntityId,
            Status = ManagementTaskStatus.New,
            ProgressPercentage = 0
        };

        dbcontext.ManagementTasks.Add(task);
        await dbcontext.SaveChangesAsync(cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Created, creatorUserId, "تم إنشاء المهمة", null, task.Status, null, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        var created = await BaseTaskQuery().FirstAsync(x => x.Id == task.Id, cancellationToken);
        return Result.Success(MapTask(created));
    }

    public async Task<Result<ManagementTaskResponse>> UpdateTaskAsync(int id, UpdateManagementTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return Result.Failure<ManagementTaskResponse>(TaskManagementErrors.TaskNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || !await dbcontext.Users.AnyAsync(x => x.Id == request.AssigneeUserId, cancellationToken))
            return Result.Failure<ManagementTaskResponse>(TaskManagementErrors.InvalidRequest);

        var fromStatus = task.Status;
        var fromAssigneeUserId = task.AssigneeUserId;
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.AssigneeUserId = request.AssigneeUserId;
        task.DueAt = request.DueAt;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.ProgressPercentage = Math.Clamp(request.ProgressPercentage, 0, 100);
        if (task.Status == ManagementTaskStatus.Completed && task.CompletedAt is null)
            task.CompletedAt = DateTime.UtcNow.AddHours(3);

        await dbcontext.SaveChangesAsync(cancellationToken);
        var actorUserId = await ResolveActorUserIdAsync(task.CreatorUserId, cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Updated, actorUserId, "تم تحديث بيانات المهمة", fromStatus, task.Status, fromAssigneeUserId, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        var updated = await BaseTaskQuery().FirstAsync(x => x.Id == task.Id, cancellationToken);
        return Result.Success(MapTask(updated));
    }

    public async Task<Result> CompleteTaskAsync(int id, CompleteTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return Result.Failure(TaskManagementErrors.TaskNotFound);

        var fromStatus = task.Status;
        task.Status = ManagementTaskStatus.Completed;
        task.ProgressPercentage = Math.Clamp(request.ProgressPercentage, 0, 100);
        if (task.ProgressPercentage < 100)
            task.ProgressPercentage = 100;
        task.CompletionNote = request.CompletionNote?.Trim();
        task.CompletedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        var actorUserId = await ResolveActorUserIdAsync(task.AssigneeUserId, cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Completed, actorUserId, task.CompletionNote, fromStatus, task.Status, task.AssigneeUserId, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RedirectTaskAsync(int id, RedirectTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return Result.Failure(TaskManagementErrors.TaskNotFound);

        if (string.IsNullOrWhiteSpace(request.NewAssigneeUserId) || !await dbcontext.Users.AnyAsync(x => x.Id == request.NewAssigneeUserId, cancellationToken))
            return Result.Failure(TaskManagementErrors.UserNotFound);

        var fromStatus = task.Status;
        var fromAssigneeUserId = task.AssigneeUserId;
        task.AssigneeUserId = request.NewAssigneeUserId;
        task.RedirectReason = request.Reason?.Trim();
        task.Status = ManagementTaskStatus.InProgress;
        await dbcontext.SaveChangesAsync(cancellationToken);
        var actorUserId = await ResolveActorUserIdAsync(task.CreatorUserId, cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Redirected, actorUserId, task.RedirectReason, fromStatus, task.Status, fromAssigneeUserId, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteTaskAsync(int id, DeleteTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return Result.Failure(TaskManagementErrors.TaskNotFound);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(TaskManagementErrors.InvalidRequest);

        var fromStatus = task.Status;
        task.Status = ManagementTaskStatus.Deleted;
        task.DeletedReason = request.Reason.Trim();
        task.DeletedAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);
        var actorUserId = await ResolveActorUserIdAsync(task.CreatorUserId, cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Deleted, actorUserId, task.DeletedReason, fromStatus, task.Status, task.AssigneeUserId, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return Result.Failure(TaskManagementErrors.TaskNotFound);

        var fromStatus = task.Status;
        task.Status = ManagementTaskStatus.InProgress;
        task.DeletedReason = null;
        task.DeletedAt = null;
        await dbcontext.SaveChangesAsync(cancellationToken);
        var actorUserId = await ResolveActorUserIdAsync(task.CreatorUserId, cancellationToken);
        await AddActivityAsync(task.Id, ManagementTaskActivityType.Restored, actorUserId, "تمت استعادة المهمة", fromStatus, task.Status, task.AssigneeUserId, task.AssigneeUserId, task.ProgressPercentage, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ManagementTaskActivityResponse>>> GetTaskActivitiesAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (!await dbcontext.ManagementTasks.AnyAsync(x => x.Id == taskId, cancellationToken))
            return Result.Failure<IEnumerable<ManagementTaskActivityResponse>>(TaskManagementErrors.TaskNotFound);

        var activities = await dbcontext.ManagementTaskActivities
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Include(x => x.FromAssigneeUser)
            .Include(x => x.ToAssigneeUser)
            .Where(x => x.ManagementTaskId == taskId)
            .OrderByDescending(x => x.ActionAt)
            .Select(x => MapActivity(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ManagementTaskActivityResponse>>(activities);
    }

    public async Task<Result<ManagementTaskActivityResponse>> AddTaskCommentAsync(int taskId, AddTaskCommentRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbcontext.ManagementTasks.FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
        if (task is null)
            return Result.Failure<ManagementTaskActivityResponse>(TaskManagementErrors.TaskNotFound);

        if (string.IsNullOrWhiteSpace(request.Comment))
            return Result.Failure<ManagementTaskActivityResponse>(TaskManagementErrors.InvalidRequest);

        var actorUserId = await ResolveActorUserIdAsync(task.AssigneeUserId, cancellationToken);
        var activity = new ManagementTaskActivity
        {
            ManagementTaskId = task.Id,
            Type = ManagementTaskActivityType.Comment,
            ActorUserId = actorUserId,
            Note = request.Comment.Trim(),
            FromStatus = task.Status,
            ToStatus = task.Status,
            FromAssigneeUserId = task.AssigneeUserId,
            ToAssigneeUserId = task.AssigneeUserId,
            ProgressPercentage = task.ProgressPercentage,
            ActionAt = DateTime.UtcNow.AddHours(3)
        };

        dbcontext.ManagementTaskActivities.Add(activity);
        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.ManagementTaskActivities
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Include(x => x.FromAssigneeUser)
            .Include(x => x.ToAssigneeUser)
            .FirstAsync(x => x.Id == activity.Id, cancellationToken);
        return Result.Success(MapActivity(saved));
    }

    public async Task<Result<IEnumerable<ApprovalRouteResponse>>> GetApprovalRoutesAsync(CancellationToken cancellationToken = default)
    {
        var routes = await dbcontext.ApprovalRoutes
            .AsNoTracking()
            .Include(x => x.Steps.OrderBy(s => s.StepOrder))
            .ThenInclude(x => x.ApproverUser)
            .OrderBy(x => x.EntityType)
            .ThenBy(x => x.NameAr)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ApprovalRouteResponse>>(routes.Select(MapRoute));
    }

    public async Task<Result<ApprovalRouteResponse>> CreateApprovalRouteAsync(CreateApprovalRouteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.EntityType) || !IsValidDeadline(request.DefaultDeadlineHours))
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.InvalidRequest);

        var route = new ApprovalRoute
        {
            NameAr = request.NameAr.Trim(),
            EntityType = request.EntityType.Trim(),
            IsActive = request.IsActive,
            DefaultDeadlineHours = request.DefaultDeadlineHours
        };

        dbcontext.ApprovalRoutes.Add(route);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRoute(route));
    }

    public async Task<Result<ApprovalRouteResponse>> UpdateApprovalRouteAsync(int id, UpdateApprovalRouteRequest request, CancellationToken cancellationToken = default)
    {
        var route = await dbcontext.ApprovalRoutes
            .Include(x => x.Steps)
            .ThenInclude(x => x.ApproverUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (route is null)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.RouteNotFound);

        if (string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.EntityType) || !IsValidDeadline(request.DefaultDeadlineHours))
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.InvalidRequest);

        var normalizedName = request.NameAr.Trim();
        var normalizedEntityType = request.EntityType.Trim();
        var duplicate = await dbcontext.ApprovalRoutes.AnyAsync(
            x => x.Id != id && x.NameAr == normalizedName && x.EntityType == normalizedEntityType,
            cancellationToken);
        if (duplicate)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.DuplicateApprovalRoute);

        route.NameAr = normalizedName;
        route.EntityType = normalizedEntityType;
        route.IsActive = request.IsActive;
        route.DefaultDeadlineHours = request.DefaultDeadlineHours;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRoute(route));
    }

    public async Task<Result<ApprovalRouteResponse>> AddApprovalStepAsync(int routeId, AddApprovalStepRequest request, CancellationToken cancellationToken = default)
    {
        var route = await dbcontext.ApprovalRoutes.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
        if (route is null)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.RouteNotFound);

        if (request.StepOrder < 1 || string.IsNullOrWhiteSpace(request.NameAr) || !await dbcontext.Users.AnyAsync(x => x.Id == request.ApproverUserId, cancellationToken))
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.InvalidRequest);

        if (route.Steps.Any(x => x.StepOrder == request.StepOrder))
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.DuplicateApprovalStepOrder);

        route.Steps.Add(new ApprovalStep
        {
            StepOrder = request.StepOrder,
            NameAr = request.NameAr.Trim(),
            ApproverUserId = request.ApproverUserId
        });

        await dbcontext.SaveChangesAsync(cancellationToken);
        var updated = await dbcontext.ApprovalRoutes.AsNoTracking().Include(x => x.Steps).ThenInclude(x => x.ApproverUser).FirstAsync(x => x.Id == routeId, cancellationToken);
        return Result.Success(MapRoute(updated));
    }

    public async Task<Result<ApprovalRouteResponse>> UpdateApprovalStepAsync(int routeId, int stepId, UpdateApprovalStepRequest request, CancellationToken cancellationToken = default)
    {
        var route = await dbcontext.ApprovalRoutes
            .Include(x => x.Steps)
            .ThenInclude(x => x.ApproverUser)
            .FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
        if (route is null)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.RouteNotFound);

        var step = route.Steps.FirstOrDefault(x => x.Id == stepId);
        if (step is null)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.ApprovalStepNotFound);

        if (request.StepOrder < 1 || string.IsNullOrWhiteSpace(request.NameAr) || !await dbcontext.Users.AnyAsync(x => x.Id == request.ApproverUserId, cancellationToken))
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.InvalidRequest);

        var duplicate = route.Steps.Any(x => x.Id != stepId && x.StepOrder == request.StepOrder);
        if (duplicate)
            return Result.Failure<ApprovalRouteResponse>(TaskManagementErrors.DuplicateApprovalStepOrder);

        step.StepOrder = request.StepOrder;
        step.NameAr = request.NameAr.Trim();
        step.ApproverUserId = request.ApproverUserId;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRoute(route));
    }

    public async Task<Result<IEnumerable<ApprovalRequestResponse>>> GetPendingApprovalRequestsAsync(string? approverUserId = null, CancellationToken cancellationToken = default)
    {
        var query = BaseApprovalRequestQuery().Where(x => x.Status == ApprovalRequestStatus.Pending);

        if (!string.IsNullOrWhiteSpace(approverUserId))
            query = query.Where(x => x.CurrentApproverUserId == approverUserId);

        var requests = await query.OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<ApprovalRequestResponse>>(requests.Select(MapApprovalRequest));
    }

    public async Task<Result<ApprovalRequestResponse>> CreateApprovalRequestAsync(CreateApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        var route = await dbcontext.ApprovalRoutes.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == request.ApprovalRouteId && x.IsActive, cancellationToken);
        if (route is null || route.Steps.Count == 0)
            return Result.Failure<ApprovalRequestResponse>(TaskManagementErrors.RouteNotFound);

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ReferenceType))
            return Result.Failure<ApprovalRequestResponse>(TaskManagementErrors.InvalidRequest);

        var approval = new ApprovalRequest
        {
            ApprovalRouteId = request.ApprovalRouteId,
            Title = request.Title.Trim(),
            ReferenceType = request.ReferenceType.Trim(),
            ReferenceId = request.ReferenceId,
            RequestedByUserId = currentUserContext.UserId ?? route.Steps.OrderBy(x => x.StepOrder).First().ApproverUserId,
            CurrentStepOrder = route.Steps.Min(x => x.StepOrder),
            CurrentApproverUserId = route.Steps.OrderBy(x => x.StepOrder).First().ApproverUserId,
            DueAt = request.DueAt ?? DateTime.UtcNow.AddHours(3).AddHours(route.DefaultDeadlineHours),
            Status = ApprovalRequestStatus.Pending
        };

        dbcontext.ApprovalRequests.Add(approval);
        QueueApprovalNotification(
            approval.CurrentApproverUserId,
            "طلب اعتماد جديد",
            $"يوجد طلب اعتماد جديد بانتظار إجراءك: {approval.Title}",
            approval.RequestedByUserId);
        await dbcontext.SaveChangesAsync(cancellationToken);
        var created = await BaseApprovalRequestQuery().FirstAsync(x => x.Id == approval.Id, cancellationToken);
        return Result.Success(MapApprovalRequest(created));
    }

    public async Task<Result<ApprovalRequestResponse?>> EnsureApprovalRequestForEntityAsync(
        string referenceType,
        int referenceId,
        string title,
        DateTime? dueAt = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referenceType) || referenceId < 1 || string.IsNullOrWhiteSpace(title))
            return Result.Failure<ApprovalRequestResponse?>(TaskManagementErrors.InvalidRequest);

        var normalizedType = referenceType.Trim();
        var existing = await BaseApprovalRequestQuery()
            .FirstOrDefaultAsync(x => x.ReferenceType == normalizedType &&
                                      x.ReferenceId == referenceId &&
                                      x.Status == ApprovalRequestStatus.Pending,
                cancellationToken);
        if (existing is not null)
            return Result.Success<ApprovalRequestResponse?>(MapApprovalRequest(existing));

        var route = await dbcontext.ApprovalRoutes
            .AsNoTracking()
            .Where(x => x.IsActive && x.EntityType == normalizedType)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (route is null)
            return Result.Success<ApprovalRequestResponse?>(null);

        var created = await CreateApprovalRequestAsync(
            new CreateApprovalRequestRequest(route.Id, title.Trim(), normalizedType, referenceId, dueAt),
            cancellationToken);
        return created.IsFailure
            ? Result.Failure<ApprovalRequestResponse?>(created.Error)
            : Result.Success<ApprovalRequestResponse?>(created.Value);
    }

    public async Task<Result> DecideApprovalRequestAsync(int id, DecideApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        var approval = await dbcontext.ApprovalRequests
            .Include(x => x.ApprovalRoute)
            .ThenInclude(x => x!.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (approval is null)
            return Result.Failure(TaskManagementErrors.ApprovalRequestNotFound);

        if (approval.Status != ApprovalRequestStatus.Pending)
            return Result.Failure(TaskManagementErrors.InvalidApprovalState);

        var currentStep = approval.ApprovalRoute?.Steps.FirstOrDefault(x => x.StepOrder == approval.CurrentStepOrder);
        if (currentStep is null || approval.CurrentApproverUserId != request.ActionByUserId)
            return Result.Failure(TaskManagementErrors.InvalidApprovalState);

        if (request.Decision == ApprovalActionDecision.Delegated ||
            request.Decision is ApprovalActionDecision.Rejected or ApprovalActionDecision.Returned && string.IsNullOrWhiteSpace(request.Comment))
            return Result.Failure(TaskManagementErrors.InvalidRequest);

        dbcontext.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = approval.Id,
            StepOrder = approval.CurrentStepOrder,
            ActionByUserId = request.ActionByUserId,
            Decision = request.Decision,
            Comment = request.Comment?.Trim(),
            ActionAt = DateTime.UtcNow.AddHours(3)
        });

        if (request.Decision == ApprovalActionDecision.Rejected)
        {
            approval.Status = ApprovalRequestStatus.Rejected;
            approval.FinalComment = request.Comment?.Trim();
            approval.ClosedAt = DateTime.UtcNow.AddHours(3);
            QueueApprovalNotification(approval.RequestedByUserId, "تم رفض طلب الاعتماد", $"تم رفض طلب الاعتماد: {approval.Title}", request.ActionByUserId);
        }
        else if (request.Decision == ApprovalActionDecision.Returned)
        {
            approval.CurrentStepOrder = approval.ApprovalRoute!.Steps.Min(x => x.StepOrder);
            approval.CurrentApproverUserId = approval.ApprovalRoute.Steps.OrderBy(x => x.StepOrder).First().ApproverUserId;
            approval.DueAt = DateTime.UtcNow.AddHours(3).AddHours(approval.ApprovalRoute.DefaultDeadlineHours);
            approval.EscalationCount = 0;
            approval.LastEscalatedAt = null;
            approval.FinalComment = request.Comment?.Trim();
            QueueApprovalNotification(approval.RequestedByUserId, "أُعيد طلب الاعتماد للمراجعة", $"أُعيد طلب الاعتماد للمراجعة: {approval.Title}", request.ActionByUserId);
            QueueApprovalNotification(approval.CurrentApproverUserId, "طلب اعتماد معاد", $"أُعيد طلب الاعتماد إلى مساره: {approval.Title}", request.ActionByUserId);
        }
        else
        {
            var nextStep = approval.ApprovalRoute!.Steps.Where(x => x.StepOrder > approval.CurrentStepOrder).OrderBy(x => x.StepOrder).FirstOrDefault();
            if (nextStep is null)
            {
                approval.Status = ApprovalRequestStatus.Approved;
                approval.FinalComment = request.Comment?.Trim();
                approval.ClosedAt = DateTime.UtcNow.AddHours(3);
                QueueApprovalNotification(approval.RequestedByUserId, "تم اعتماد الطلب", $"تم اعتماد طلبك: {approval.Title}", request.ActionByUserId);
            }
            else
            {
                approval.CurrentStepOrder = nextStep.StepOrder;
                approval.CurrentApproverUserId = nextStep.ApproverUserId;
                approval.DueAt = DateTime.UtcNow.AddHours(3).AddHours(approval.ApprovalRoute.DefaultDeadlineHours);
                approval.EscalationCount = 0;
                approval.LastEscalatedAt = null;
                QueueApprovalNotification(nextStep.ApproverUserId, "طلب اعتماد بانتظار الإجراء", $"انتقل طلب الاعتماد إلى خطوتك: {approval.Title}", request.ActionByUserId);
            }
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CancelApprovalRequestAsync(int id, CancelApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        var approval = await dbcontext.ApprovalRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (approval is null)
            return Result.Failure(TaskManagementErrors.ApprovalRequestNotFound);

        if (approval.Status != ApprovalRequestStatus.Pending || approval.RequestedByUserId != request.RequestedByUserId)
            return Result.Failure(TaskManagementErrors.InvalidApprovalState);

        approval.Status = ApprovalRequestStatus.Cancelled;
        approval.FinalComment = request.Comment?.Trim();
        approval.ClosedAt = DateTime.UtcNow.AddHours(3);
        QueueApprovalNotification(approval.CurrentApproverUserId, "تم إلغاء طلب اعتماد", $"أُلغي طلب الاعتماد: {approval.Title}", request.RequestedByUserId);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DelegateApprovalRequestAsync(int id, DelegateApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        var approval = await dbcontext.ApprovalRequests
            .Include(x => x.ApprovalRoute)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (approval is null)
            return Result.Failure(TaskManagementErrors.ApprovalRequestNotFound);

        if (approval.Status != ApprovalRequestStatus.Pending || approval.CurrentApproverUserId != request.ActionByUserId ||
            string.IsNullOrWhiteSpace(request.Reason) || string.IsNullOrWhiteSpace(request.DelegateToUserId) ||
            request.DelegateToUserId == request.ActionByUserId ||
            !await dbcontext.Users.AnyAsync(x => x.Id == request.DelegateToUserId && x.IsActive, cancellationToken))
        {
            return Result.Failure(TaskManagementErrors.InvalidApprovalState);
        }

        dbcontext.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = approval.Id,
            StepOrder = approval.CurrentStepOrder,
            ActionByUserId = request.ActionByUserId,
            DelegatedToUserId = request.DelegateToUserId,
            Decision = ApprovalActionDecision.Delegated,
            Comment = request.Reason.Trim(),
            ActionAt = DateTime.UtcNow.AddHours(3)
        });

        approval.CurrentApproverUserId = request.DelegateToUserId;
        approval.DueAt = DateTime.UtcNow.AddHours(3).AddHours(approval.ApprovalRoute?.DefaultDeadlineHours ?? 72);
        approval.EscalationCount = 0;
        approval.LastEscalatedAt = null;
        QueueApprovalNotification(request.DelegateToUserId, "تم تفويض طلب اعتماد", $"تم تفويض طلب الاعتماد إليك: {approval.Title}", request.ActionByUserId);
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<int>> EscalateOverdueApprovalRequestsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.AddHours(3);
        var overdue = await dbcontext.ApprovalRequests
            .Where(x => x.Status == ApprovalRequestStatus.Pending && x.DueAt != null && x.DueAt <= now && x.EscalationCount == 0)
            .ToListAsync(cancellationToken);

        foreach (var approval in overdue)
        {
            approval.EscalationCount = 1;
            approval.LastEscalatedAt = now;
            QueueApprovalNotification(approval.CurrentApproverUserId, "طلب اعتماد متأخر", $"تأخر طلب الاعتماد: {approval.Title}", approval.RequestedByUserId);
        }

        if (overdue.Count > 0)
            await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(overdue.Count);
    }

    private void QueueApprovalNotification(string recipientUserId, string title, string body, string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId) || string.IsNullOrWhiteSpace(createdByUserId))
            return;

        dbcontext.SystemNotifications.Add(new SystemNotification
        {
            Title = title,
            Body = body,
            Channel = MessageChannel.Internal,
            Status = NotificationStatus.Active,
            CreatedBySystemUserId = createdByUserId,
            Recipients =
            {
                new SystemNotificationRecipient { RecipientUserId = recipientUserId, DeliveryStatus = ChannelDeliveryStatus.Pending }
            }
        });
    }

    private async Task AddActivityAsync(
        int taskId,
        ManagementTaskActivityType type,
        string actorUserId,
        string? note,
        ManagementTaskStatus? fromStatus,
        ManagementTaskStatus? toStatus,
        string? fromAssigneeUserId,
        string? toAssigneeUserId,
        int? progressPercentage,
        CancellationToken cancellationToken)
    {
        dbcontext.ManagementTaskActivities.Add(new ManagementTaskActivity
        {
            ManagementTaskId = taskId,
            Type = type,
            ActorUserId = actorUserId,
            Note = note,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            FromAssigneeUserId = fromAssigneeUserId,
            ToAssigneeUserId = toAssigneeUserId,
            ProgressPercentage = progressPercentage,
            ActionAt = DateTime.UtcNow.AddHours(3)
        });

        await dbcontext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ResolveActorUserIdAsync(string fallbackUserId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(currentUserContext.UserId) &&
            await dbcontext.Users.AnyAsync(x => x.Id == currentUserContext.UserId, cancellationToken))
        {
            return currentUserContext.UserId;
        }

        return fallbackUserId;
    }

    private static bool IsValidDeadline(int hours) => hours is >= 1 and <= 720;

    private IQueryable<ManagementTask> BaseTaskQuery() =>
        dbcontext.ManagementTasks
            .AsNoTracking()
            .Include(x => x.CreatorUser)
            .Include(x => x.AssigneeUser);

    private IQueryable<ApprovalRequest> BaseApprovalRequestQuery() =>
        dbcontext.ApprovalRequests
            .AsNoTracking()
            .Include(x => x.ApprovalRoute)
            .ThenInclude(x => x!.Steps)
            .ThenInclude(x => x.ApproverUser)
            .Include(x => x.RequestedByUser)
            .Include(x => x.CurrentApproverUser);

    private static ManagementTaskResponse MapTask(ManagementTask task) =>
        new(
            task.Id,
            task.Title,
            task.Description,
            task.CreatorUserId,
            task.CreatorUser?.FullName ?? string.Empty,
            task.AssigneeUserId,
            task.AssigneeUser?.FullName ?? string.Empty,
            task.DueAt,
            task.Priority.ToString(),
            task.Status.ToString(),
            task.ProgressPercentage,
            task.RelatedEntityType,
            task.RelatedEntityId,
            task.CompletionNote,
            task.CompletedAt,
            task.RedirectReason,
            task.DeletedReason,
            task.DeletedAt,
            task.CreatedAt);

    private static ManagementTaskActivityResponse MapActivity(ManagementTaskActivity activity) =>
        new(
            activity.Id,
            activity.ManagementTaskId,
            activity.Type.ToString(),
            activity.ActorUserId,
            activity.ActorUser?.FullName ?? string.Empty,
            activity.Note,
            activity.FromStatus?.ToString(),
            activity.ToStatus?.ToString(),
            activity.FromAssigneeUserId,
            activity.FromAssigneeUser?.FullName,
            activity.ToAssigneeUserId,
            activity.ToAssigneeUser?.FullName,
            activity.ProgressPercentage,
            activity.ActionAt);

    private static ApprovalRouteResponse MapRoute(ApprovalRoute route) =>
        new(
            route.Id,
            route.NameAr,
            route.EntityType,
            route.IsActive,
            route.DefaultDeadlineHours,
            route.Steps.OrderBy(x => x.StepOrder).Select(x => new ApprovalStepResponse(x.Id, x.StepOrder, x.NameAr, x.ApproverUserId, x.ApproverUser?.FullName ?? string.Empty)).ToList());

    private static ApprovalRequestResponse MapApprovalRequest(ApprovalRequest request)
    {
        var currentApprover = request.CurrentApproverUser?.FullName ?? request.ApprovalRoute?.Steps.FirstOrDefault(x => x.StepOrder == request.CurrentStepOrder)?.ApproverUser?.FullName;
        return new ApprovalRequestResponse(
            request.Id,
            request.ApprovalRouteId,
            request.ApprovalRoute?.NameAr ?? string.Empty,
            request.Title,
            request.ReferenceType,
            request.ReferenceId,
            request.RequestedByUserId,
            request.RequestedByUser?.FullName ?? string.Empty,
            request.Status.ToString(),
            request.CurrentStepOrder,
            currentApprover,
            request.FinalComment,
            request.DueAt,
            request.LastEscalatedAt,
            request.EscalationCount,
            request.ClosedAt,
            request.CreatedAt);
    }
}
