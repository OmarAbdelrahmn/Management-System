using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class MeetingErrors
{
    public static readonly Error NotFound = new("Meeting.NotFound", "Meeting was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AgendaItemNotFound = new("Meeting.AgendaItemNotFound", "Agenda item was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidDeadline = new("Meeting.InvalidDeadline", "Acceptance deadline must be greater than zero.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidMeetingDate = new("Meeting.InvalidMeetingDate", "Meeting date must fall inside the board cycle.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidMeetingType = new("Meeting.InvalidMeetingType", "Meeting archive type must be general, board, or assembly.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidWorkflowState = new("Meeting.InvalidWorkflowState", "Meeting cannot move to the requested workflow state.", StatusCodes.Status409Conflict);
    public static readonly Error InvitationsAlreadySent = new("Meeting.InvitationsAlreadySent", "Meeting invitations were already sent.", StatusCodes.Status409Conflict);
    public static readonly Error ReadOnlyMinute = new("Meeting.ReadOnlyMinute", "Meeting minute is read-only after chairman signature.", StatusCodes.Status409Conflict);
}
