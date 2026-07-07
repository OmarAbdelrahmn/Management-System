using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class EmailErrors
{
    public static readonly Error SmtpNotConfigured =
        new("Email.SmtpNotConfigured", "SMTP email and password must be configured before sending emails.", StatusCodes.Status400BadRequest);

    public static readonly Error OutboxMessageNotFound =
        new("Email.OutboxMessageNotFound", "Email outbox message was not found.", StatusCodes.Status404NotFound);
}
