using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class VotingErrors
{
    public static readonly Error InvalidRequest = new("Voting.InvalidRequest", "Voting request is invalid.", StatusCodes.Status400BadRequest);
    public static readonly Error NotFound = new("Voting.NotFound", "Vote session was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AgendaItemDoesNotRequireDecision = new("Voting.NotDecisionItem", "This agenda item does not require a decision.", StatusCodes.Status400BadRequest);
    public static readonly Error AlreadyOpen = new("Voting.AlreadyOpen", "A vote session is already open for this item.", StatusCodes.Status409Conflict);
    public static readonly Error Closed = new("Voting.Closed", "Vote session is closed.", StatusCodes.Status409Conflict);
    public static readonly Error MemberNotPresent = new("Voting.MemberNotPresent", "Only accepted attendees can vote.", StatusCodes.Status403Forbidden);
}
