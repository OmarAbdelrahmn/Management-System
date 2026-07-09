using Application.Contracts.TaskManagement;
using Application.Service.TaskManagement;
using Domain.Entities;
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
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var result = await taskService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] ManagementTaskStatus? status, [FromQuery] string? assigneeUserId, [FromQuery] bool includeDeleted, CancellationToken cancellationToken)
    {
        var result = await taskService.SearchTasksAsync(new TaskSearchRequest(search, status, assigneeUserId, includeDeleted), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var result = await taskService.GetMyTasksAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetTaskAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Create([FromBody] CreateManagementTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateTaskAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManagementTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.UpdateTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CompleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/redirect")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Redirect(int id, [FromBody] RedirectTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.RedirectTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/delete")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Delete(int id, [FromBody] DeleteTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteTaskAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "Admin,Secretary,Chairman")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.RestoreTaskAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{id:int}/activities")]
    public async Task<IActionResult> Activities(int id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetTaskActivitiesAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddTaskCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.AddTaskCommentAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
