using Application.Contracts.ElectronicOffice;
using Application.Service.ElectronicOffice;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class ElectronicOfficeController(IElectronicOfficeService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] OfficeRecordStatus? status, CancellationToken ct) => ToAction(await service.GetAttendanceAsync(status, ct));
    [HttpPost("attendance")]
    public async Task<IActionResult> SaveAttendance([FromBody] SaveOfficeAttendanceRequest request, CancellationToken ct) => ToAction(await service.SaveAttendanceAsync(request, ct));
    [HttpGet("reminders")]
    public async Task<IActionResult> Reminders([FromQuery] OfficeRecordStatus? status, CancellationToken ct) => ToAction(await service.GetRemindersAsync(status, ct));
    [HttpPost("reminders")]
    public async Task<IActionResult> SaveReminder([FromBody] SaveOfficeReminderRequest request, CancellationToken ct) => ToAction(await service.SaveReminderAsync(null, request, ct));
    [HttpPut("reminders/{id:int}")]
    public async Task<IActionResult> UpdateReminder(int id, [FromBody] SaveOfficeReminderRequest request, CancellationToken ct) => ToAction(await service.SaveReminderAsync(id, request, ct));
    [HttpGet("requests")]
    public async Task<IActionResult> Requests([FromQuery] OfficeRequestType? type, [FromQuery] OfficeRecordStatus? status, CancellationToken ct) => ToAction(await service.GetRequestsAsync(type, status, ct));
    [HttpPost("requests")]
    public async Task<IActionResult> SaveRequest([FromBody] SaveOfficeAdministrativeRequestRequest request, CancellationToken ct) => ToAction(await service.SaveRequestAsync(null, request, ct));
    [HttpPut("requests/{id:int}")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] SaveOfficeAdministrativeRequestRequest request, CancellationToken ct) => ToAction(await service.SaveRequestAsync(id, request, ct));
    [HttpPost("requests/{id:int}/decide")]
    public async Task<IActionResult> DecideRequest(int id, [FromBody] DecideOfficeRequestRequest request, CancellationToken ct)
    {
        var result = await service.DecideRequestAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] OfficeTransactionStatus? status, CancellationToken ct) => ToAction(await service.GetTransactionsAsync(status, ct));
    [HttpPost("transactions")]
    public async Task<IActionResult> SaveTransaction([FromBody] SaveOfficeTransactionRequest request, CancellationToken ct) => ToAction(await service.SaveTransactionAsync(null, request, ct));
    [HttpPut("transactions/{id:int}")]
    public async Task<IActionResult> UpdateTransactionRecord(int id, [FromBody] SaveOfficeTransactionRequest request, CancellationToken ct) => ToAction(await service.SaveTransactionAsync(id, request, ct));
    [HttpPost("transactions/{id:int}/status")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateOfficeTransactionStatusRequest request, CancellationToken ct)
    {
        var result = await service.UpdateTransactionStatusAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    [HttpGet("logs")]
    public async Task<IActionResult> Logs([FromQuery] OfficeLogType? type, CancellationToken ct) => ToAction(await service.GetLogRecordsAsync(type, ct));
    [HttpPost("logs")]
    public async Task<IActionResult> SaveLog([FromBody] SaveOfficeLogRecordRequest request, CancellationToken ct) => ToAction(await service.SaveLogRecordAsync(null, request, ct));
    [HttpPut("logs/{id:int}")]
    public async Task<IActionResult> UpdateLog(int id, [FromBody] SaveOfficeLogRecordRequest request, CancellationToken ct) => ToAction(await service.SaveLogRecordAsync(id, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
