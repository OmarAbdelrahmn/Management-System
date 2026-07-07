using Application.Contracts.Invitations;
using Application.Contracts.Meetings;
using Application.Contracts.Minutes;
using Application.Contracts.Voting;
using Application.Service.Realtime;
using Express_Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Express_Service.Realtime;

public class SignalRMeetingRealtimeNotifier(IHubContext<MeetingHub> hubContext) : IMeetingRealtimeNotifier
{
    public Task AgendaItemAddedAsync(int meetingId, AgendaItemResponse agendaItem, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "agendaItemAdded", agendaItem, cancellationToken);

    public Task MeetingStartedAsync(int meetingId, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "meetingStarted", new { MeetingId = meetingId }, cancellationToken);

    public Task MeetingApprovedAsync(int meetingId, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "meetingApproved", new { MeetingId = meetingId }, cancellationToken);

    public Task MeetingFinishedAsync(int meetingId, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "meetingFinished", new { MeetingId = meetingId }, cancellationToken);

    public Task InvitationsSentAsync(int meetingId, IEnumerable<InvitationResponse> invitations, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "invitationsSent", new { MeetingId = meetingId, Invitations = invitations }, cancellationToken);

    public Task InvitationAcceptedAsync(int meetingId, InvitationResponse invitation, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "invitationAccepted", invitation, cancellationToken);

    public Task VoteOpenedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "voteOpened", voteSession, cancellationToken);

    public Task VoteUpdatedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "voteUpdated", voteSession, cancellationToken);

    public Task VoteClosedAsync(int meetingId, VoteSessionResponse voteSession, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "voteClosed", voteSession, cancellationToken);

    public Task MinutePublishedAsync(int meetingId, MinuteResponse minute, CancellationToken cancellationToken = default) =>
        SendAsync(meetingId, "minutePublished", minute, cancellationToken);

    private Task SendAsync(int meetingId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(MeetingHub.MeetingGroup(meetingId)).SendAsync(method, payload, cancellationToken);
}
