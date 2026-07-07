using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class InvitationErrors
{
    public static readonly Error NotFound = new("Invitation.NotFound", "Invitation was not found.", StatusCodes.Status404NotFound);
    public static readonly Error Expired = new("Invitation.Expired", "Invitation acceptance deadline has expired.", StatusCodes.Status409Conflict);
    public static readonly Error AgendaAcknowledgementRequired = new("Invitation.AgendaAcknowledgementRequired", "Agenda read acknowledgement is required before entering the meeting.", StatusCodes.Status400BadRequest);
}
