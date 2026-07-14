using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class AttachmentErrors
{
    public static readonly Error NotFound = new("Attachment.NotFound", "Attachment was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidRequest = new("Attachment.InvalidRequest", "Attachment request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error UnsupportedType = new("Attachment.UnsupportedType", "Attachment type is not supported.", StatusCodes.Status400BadRequest);
    public static readonly Error TooLarge = new("Attachment.TooLarge", "Attachment exceeds the permitted size.", StatusCodes.Status413PayloadTooLarge);
    public static readonly Error MalwareScanFailed = new("Attachment.MalwareScanFailed", "Attachment security scan did not complete.", StatusCodes.Status503ServiceUnavailable);
    public static readonly Error Infected = new("Attachment.Infected", "Attachment was rejected by the security scan.", StatusCodes.Status400BadRequest);
    public static readonly Error Unavailable = new("Attachment.Unavailable", "Attachment content is unavailable.", StatusCodes.Status410Gone);
    public static readonly Error Forbidden = new("Attachment.Forbidden", "You do not have permission to access this attachment.", StatusCodes.Status403Forbidden);
    public static readonly Error UnsupportedEntity = new("Attachment.UnsupportedEntity", "Attachments are not enabled for the requested record type.", StatusCodes.Status400BadRequest);
}
