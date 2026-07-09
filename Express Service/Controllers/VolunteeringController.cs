using Application.Contracts.Volunteering;
using Application.Service.Volunteering;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class VolunteeringController(IVolunteeringService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] VolunteerUserStatus? status, CancellationToken ct) => ToAction(await service.GetUsersAsync(status, ct));
    [HttpPost("users")]
    public async Task<IActionResult> SaveUser([FromBody] SaveVolunteerUserRequest request, CancellationToken ct) => ToAction(await service.SaveUserAsync(null, request, ct));
    [HttpGet("requests")]
    public async Task<IActionResult> Requests([FromQuery] VolunteerRequestSource? source, [FromQuery] VolunteerRequestStatus? status, CancellationToken ct) => ToAction(await service.GetRequestsAsync(source, status, ct));
    [HttpPost("requests")]
    public async Task<IActionResult> SaveRequest([FromBody] SaveVolunteerRequestRequest request, CancellationToken ct) => ToAction(await service.SaveRequestAsync(null, request, ct));
    [HttpPost("requests/{id:int}/status")]
    public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateVolunteerRequestStatusRequest request, CancellationToken ct)
    {
        var result = await service.UpdateRequestStatusAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("opportunities")]
    public async Task<IActionResult> Opportunities([FromQuery] VolunteerOpportunityStatus? status, CancellationToken ct) => ToAction(await service.GetOpportunitiesAsync(status, ct));
    [HttpPost("opportunities")]
    public async Task<IActionResult> SaveOpportunity([FromBody] SaveVolunteerOpportunityRequest request, CancellationToken ct) => ToAction(await service.SaveOpportunityAsync(null, request, ct));
    [HttpPost("opportunities/{id:int}/report")]
    public async Task<IActionResult> SaveOpportunityReport(int id, [FromBody] SaveVolunteerOpportunityReportRequest request, CancellationToken ct)
    {
        var result = await service.SaveOpportunityReportAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks([FromQuery] int? opportunityId, [FromQuery] VolunteerTaskStatus? status, CancellationToken ct) => ToAction(await service.GetTasksAsync(opportunityId, status, ct));
    [HttpPost("tasks")]
    public async Task<IActionResult> SaveTask([FromBody] SaveVolunteerOpportunityTaskRequest request, CancellationToken ct) => ToAction(await service.SaveTaskAsync(null, request, ct));
    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] int? opportunityId, [FromQuery] int? volunteerUserId, CancellationToken ct) => ToAction(await service.GetAttendanceAsync(opportunityId, volunteerUserId, ct));
    [HttpPost("attendance")]
    public async Task<IActionResult> SaveAttendance([FromBody] SaveVolunteerAttendanceRequest request, CancellationToken ct) => ToAction(await service.SaveAttendanceAsync(null, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
