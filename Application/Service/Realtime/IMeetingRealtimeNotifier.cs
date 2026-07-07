using Application.Contracts.Invitations;
using Application.Contracts.Meetings;
using Application.Contracts.Minutes;
using Application.Contracts.Voting;

namespace Application.Service.Realtime;

public interface IMeetingRealtimeNotifier
{
    Task AgendaItemAddedAsync(int meetingId, AgendaItemResponse agendaItem, CancellationToken cancellationToken = default);
    Task MeetingStartedAsync(int meetingId, CancellationToken cancellationToken = default);
    Task MeetingApprovedAsync(int meetingId, CancellationToken cancellationToken = default);
    Task MeetingFinishedAsync(int meetingId, CancellationToken cancellationToken = default);
    Task InvitationsSentAsync(int meetingId, IEnumerable<InvitationResponse> invitations, CancellationToken cancellationToken = default);
    Task InvitationAcceptedAsync(int meetingId, InvitationResponse invitation, CancellationToken cancellationToken = default);
    Task VoteOpenedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default);
    Task VoteUpdatedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default);
    Task VoteClosedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default);
    Task MinutePublishedAsync(int meetingId, MinuteResponse minute, CancellationToken cancellationToken = default);
}
