using Application.Abstraction;
using Application.Contracts.TaskManagement;
using Domain.Entities;

namespace Application.Service.TaskManagement;

public interface ITaskManagementService
{
    Task<Result<IEnumerable<UserPickerResponse>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ManagementTaskResponse>>> SearchTasksAsync(TaskSearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ManagementTaskResponse>>> GetMyTasksAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<ManagementTaskResponse>> GetTaskAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ManagementTaskResponse>> CreateTaskAsync(CreateManagementTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<ManagementTaskResponse>> UpdateTaskAsync(int id, UpdateManagementTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> CompleteTaskAsync(int id, CompleteTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> RedirectTaskAsync(int id, RedirectTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteTaskAsync(int id, DeleteTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> RestoreTaskAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ManagementTaskActivityResponse>>> GetTaskActivitiesAsync(int taskId, CancellationToken cancellationToken = default);
    Task<Result<ManagementTaskActivityResponse>> AddTaskCommentAsync(int taskId, AddTaskCommentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ApprovalRouteResponse>>> GetApprovalRoutesAsync(CancellationToken cancellationToken = default);
    Task<Result<ApprovalRouteResponse>> CreateApprovalRouteAsync(CreateApprovalRouteRequest request, CancellationToken cancellationToken = default);
    Task<Result<ApprovalRouteResponse>> AddApprovalStepAsync(int routeId, AddApprovalStepRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ApprovalRequestResponse>>> GetPendingApprovalRequestsAsync(string? approverUserId = null, CancellationToken cancellationToken = default);
    Task<Result<ApprovalRequestResponse>> CreateApprovalRequestAsync(CreateApprovalRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result> DecideApprovalRequestAsync(int id, DecideApprovalRequestRequest request, CancellationToken cancellationToken = default);
}
