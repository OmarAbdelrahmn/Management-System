using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Domain.Entities;
using System.Security.Claims;

namespace Express_Service.Services;

public class TaskUiService(ITaskManagementService taskService, IHttpContextAccessor httpContextAccessor)
{
    public string? CurrentUserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<List<UserPickerResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = await taskService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<ManagementTaskResponse>> SearchTasksAsync(string? search = null, ManagementTaskStatus? status = null, string? assigneeUserId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var result = await taskService.SearchTasksAsync(new TaskSearchRequest(search, status, assigneeUserId, includeDeleted), cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<List<ManagementTaskResponse>> GetMyTasksAsync(CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId ?? string.Empty;
        var result = await taskService.GetMyTasksAsync(userId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateTaskAsync(CreateManagementTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.CreateTaskAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء المهمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateTaskAsync(int id, UpdateManagementTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.UpdateTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث المهمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CompleteTaskAsync(int id, CompleteTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.CompleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنجاز المهمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RedirectTaskAsync(int id, RedirectTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.RedirectTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحويل المهمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DeleteTaskAsync(int id, DeleteTaskRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.DeleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم حذف المهمة.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> RestoreTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await taskService.RestoreTaskAsync(id, cancellationToken);
        return result.IsSuccess ? (true, "تمت استعادة المهمة.") : (false, result.Error.Description);
    }

    public async Task<List<ManagementTaskActivityResponse>> GetTaskActivitiesAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var result = await taskService.GetTaskActivitiesAsync(taskId, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> AddTaskCommentAsync(int taskId, string comment, CancellationToken cancellationToken = default)
    {
        var result = await taskService.AddTaskCommentAsync(taskId, new AddTaskCommentRequest(comment), cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة التعليق.") : (false, result.Error.Description);
    }

    public async Task<List<ApprovalRouteResponse>> GetApprovalRoutesAsync(CancellationToken cancellationToken = default)
    {
        var result = await taskService.GetApprovalRoutesAsync(cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateApprovalRouteAsync(CreateApprovalRouteRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.CreateApprovalRouteAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إنشاء مسار الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateApprovalRouteAsync(int id, UpdateApprovalRouteRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.UpdateApprovalRouteAsync(id, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث مسار الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> AddApprovalStepAsync(int routeId, AddApprovalStepRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.AddApprovalStepAsync(routeId, request, cancellationToken);
        return result.IsSuccess ? (true, "تمت إضافة خطوة الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> UpdateApprovalStepAsync(int routeId, int stepId, UpdateApprovalStepRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.UpdateApprovalStepAsync(routeId, stepId, request, cancellationToken);
        return result.IsSuccess ? (true, "تم تحديث خطوة الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<List<ApprovalRequestResponse>> GetPendingApprovalsAsync(bool mineOnly = false, CancellationToken cancellationToken = default)
    {
        var result = await taskService.GetPendingApprovalRequestsAsync(mineOnly ? CurrentUserId : null, cancellationToken);
        return result.IsSuccess ? result.Value.ToList() : [];
    }

    public async Task<(bool Success, string Message)> CreateApprovalRequestAsync(CreateApprovalRequestRequest request, CancellationToken cancellationToken = default)
    {
        var result = await taskService.CreateApprovalRequestAsync(request, cancellationToken);
        return result.IsSuccess ? (true, "تم إرسال طلب الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DecideApprovalAsync(int id, ApprovalActionDecision decision, string? comment, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId ?? string.Empty;
        var result = await taskService.DecideApprovalRequestAsync(id, new DecideApprovalRequestRequest(userId, decision, comment), cancellationToken);
        return result.IsSuccess ? (true, "تم تسجيل قرار الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> DelegateApprovalAsync(int id, string delegateToUserId, string reason, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId ?? string.Empty;
        var result = await taskService.DelegateApprovalRequestAsync(id, new DelegateApprovalRequestRequest(userId, delegateToUserId, reason), cancellationToken);
        return result.IsSuccess ? (true, "تم تفويض طلب الاعتماد.") : (false, result.Error.Description);
    }

    public async Task<(bool Success, string Message)> CancelApprovalAsync(int id, string? comment, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId ?? string.Empty;
        var result = await taskService.CancelApprovalRequestAsync(id, new CancelApprovalRequestRequest(userId, comment), cancellationToken);
        return result.IsSuccess ? (true, "تم إلغاء طلب الاعتماد.") : (false, result.Error.Description);
    }
}
