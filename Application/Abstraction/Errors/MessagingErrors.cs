using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class MessagingErrors
{
    public static readonly Error NotFound =
        new("Messaging.NotFound", "Message was not found.", StatusCodes.Status404NotFound);

    public static readonly Error NotificationNotFound =
        new("Messaging.NotificationNotFound", "Notification was not found.", StatusCodes.Status404NotFound);

    public static readonly Error RecipientNotFound =
        new("Messaging.RecipientNotFound", "Notification recipient was not found.", StatusCodes.Status404NotFound);

    public static readonly Error TemplateNotFound =
        new("Messaging.TemplateNotFound", "Message template was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidRequest =
        new("Messaging.InvalidRequest", "Messaging request is invalid.", StatusCodes.Status400BadRequest);

    public static readonly Error UserNotFound =
        new("Messaging.UserNotFound", "One or more recipients were not found.", StatusCodes.Status404NotFound);

    public static readonly Error NotificationCancelled =
        new("Messaging.NotificationCancelled", "Cancelled notifications cannot be delivered.", StatusCodes.Status409Conflict);
}
