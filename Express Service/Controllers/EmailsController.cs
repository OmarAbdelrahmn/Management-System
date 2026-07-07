using Application.Service.Emails;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class EmailsController(IEmailService service, IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    [HttpGet("smtp-status")]
    public IActionResult SmtpStatus()
    {
        var result = service.GetSmtpConfigurationStatus();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> Outbox([FromQuery] bool? sent, CancellationToken cancellationToken)
    {
        var result = await service.GetOutboxAsync(sent, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("outbox/send-pending")]
    public async Task<IActionResult> SendPending([FromQuery] int maxMessages = 50, CancellationToken cancellationToken = default)
    {
        var result = await service.SendPendingAsync(maxMessages, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("outbox/enqueue-send-pending")]
    public IActionResult EnqueueSendPending()
    {
        var jobId = backgroundJobClient.Enqueue<IEmailBackgroundJob>(
            "emails",
            job => job.SendPendingEmailsAsync());

        return Ok(new { JobId = jobId });
    }

    [HttpPost("outbox/{id:int}/send")]
    public async Task<IActionResult> SendOne(int id, CancellationToken cancellationToken)
    {
        var result = await service.SendOutboxMessageAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
