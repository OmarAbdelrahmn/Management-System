using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Domain.Entities;
using Express_Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController(ITaskManagementService taskService) : ControllerBase
{
    [HttpGet("users")]
    [RequirePermission("system.electronic-office.common_tasks_create")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var result = await taskService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    [RequirePermission("system.electronic-office.common_tasks_update")]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] ManagementTaskStatus? status, [FromQuery] string? assigneeUserId, [FromQuery] bool includeDeleted, CancellationToken cancellationToken)
    {
        var result = await taskService.SearchTasksAsync(new TaskSearchRequest(search, status, assigneeUserId, includeDeleted), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("mine")]
    [RequirePermission("system.electronic-office.common_tasks")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await taskService.GetMyTasksAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    [RequirePermission("system.electronic-office.common_tasks")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetTaskAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [RequirePermission("system.electronic-office.common_tasks_create")]
    public async Task<IActionResult> Create([FromBody] CreateManagementTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateTaskAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    [RequirePermission("system.electronic-office.common_tasks_update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManagementTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.UpdateTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/complete")]
    [RequirePermission("system.electronic-office.common_tasks_complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CompleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/redirect")]
    [RequirePermission("system.electronic-office.common_tasks_update")]
    public async Task<IActionResult> Redirect(int id, [FromBody] RedirectTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.RedirectTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/delete")]
    [RequirePermission("system.electronic-office.common_tasks_remove")]
    public async Task<IActionResult> Delete(int id, [FromBody] DeleteTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/restore")]
    [RequirePermission("system.electronic-office.common_tasks_remove")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.RestoreTaskAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{id:int}/activities")]
    [RequirePermission("system.electronic-office.common_tasks")]
    public async Task<IActionResult> Activities(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetTaskActivitiesAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/comments")]
    [RequirePermission("system.electronic-office.common_tasks")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddTaskCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.AddTaskCommentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
