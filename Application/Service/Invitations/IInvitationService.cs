using Application.Abstraction;
using Application.Contracts.Invitations;

namespace Application.Service.Invitations;

public interface IInvitationService
{
    Task<Result<IEnumerable<InvitationResponse>>> SendMeetingInvitationsAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<Result<InvitationResponse>> AcceptAsync(int invitationId, AcceptInvitationRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<InvitationResponse>>> GetMeetingInvitationsAsync(int meetingId, CancellationToken cancellationToken = default);
}
