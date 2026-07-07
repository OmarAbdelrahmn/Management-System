using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class MinuteErrors
{
    public static readonly Error NotFound = new("Minute.NotFound", "Meeting minute was not found.", StatusCodes.Status404NotFound);
    public static readonly Error NoApprovedDecisions = new("Minute.NoApprovedDecisions", "No approved decisions are waiting for chairman signature.", StatusCodes.Status400BadRequest);
}
