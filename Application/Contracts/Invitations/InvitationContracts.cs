using Application.Contracts.Meetings;

namespace Application.Contracts.Invitations;

public record InvitationResponse(
    int Id,
    int BoardMeetingId,
    string MemberUserId,
    string Status,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    bool AgendaReadAcknowledged);

public record AcceptInvitationRequest(
    bool AgendaReadAcknowledged,
    MeetingNoteRequest? Note);
