using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Invitations;
using Application.Service.Permissions;
using Application.Service.Realtime;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Invitations;

public class InvitationService(
    ApplicationDbcontext dbcontext,
    IBoardAccessService? boardAccessService = null,
    IMeetingRealtimeNotifier? realtimeNotifier = null) : IInvitationService
{
    public async Task<Result<IEnumerable<InvitationResponse>>> SendMeetingInvitationsAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.BoardCycle)
            .ThenInclude(x => x!.Board)
            .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<IEnumerable<InvitationResponse>>(MeetingErrors.NotFound);

        if (meeting.Status != MeetingStatus.Draft)
            return Result.Failure<IEnumerable<InvitationResponse>>(MeetingErrors.InvitationsAlreadySent);

        var boardId = meeting.BoardCycle!.BoardId;
        if (boardAccessService is not null && !await boardAccessService.CanManageBoardAsync(boardId, cancellationToken))
            return Result.Failure<IEnumerable<InvitationResponse>>(PermissionErrors.Forbidden);

        var members = await dbcontext.BoardMemberships
            .AsNoTracking()
            .Where(x => x.BoardId == boardId && x.IsActive)
            .ToListAsync(cancellationToken);

        var existingMemberIds = await dbcontext.MeetingInvitations
            .Where(x => x.BoardMeetingId == meetingId)
            .Select(x => x.MemberUserId)
            .ToListAsync(cancellationToken);

        var users = await dbcontext.Users
            .AsNoTracking()
            .Where(x => members.Select(m => m.UserId).Contains(x.Id))
            .Select(x => new { x.Id, x.Email, x.FullName })
            .ToListAsync(cancellationToken);

        var expiresAt = DateTime.UtcNow.AddHours(3).AddDays(meeting.AcceptanceDeadlineDays);
        foreach (var member in members.Where(x => !existingMemberIds.Contains(x.UserId)))
        {
            dbcontext.MeetingInvitations.Add(new MeetingInvitation
            {
                BoardMeetingId = meetingId,
                MemberUserId = member.UserId,
                ExpiresAt = expiresAt
            });

            var user = users.FirstOrDefault(x => x.Id == member.UserId);
            if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                dbcontext.EmailOutbox.Add(new EmailOutbox
                {
                    ToEmail = user.Email,
                    Subject = $"دعوة اجتماع: {meeting.Title}",
                    Body = $"سعادة العضو/ {user.FullName}، نأمل فتح التطبيق وتأكيد الحضور لاجتماع {meeting.Title} قبل انتهاء مهلة القبول. لا يحتوي هذا البريد على روابط خارجية."
                });
            }
        }

        meeting.Status = MeetingStatus.InvitationsSent;
        meeting.InvitationsSentAt = DateTime.UtcNow.AddHours(3);
        await dbcontext.SaveChangesAsync(cancellationToken);

        var invitationsResult = await GetMeetingInvitationsAsync(meetingId, cancellationToken);
        if (invitationsResult.IsSuccess && realtimeNotifier is not null)
            await realtimeNotifier.InvitationsSentAsync(meetingId, invitationsResult.Value, cancellationToken);

        return invitationsResult;
    }

    public async Task<Result<InvitationResponse>> AcceptAsync(int invitationId, AcceptInvitationRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.AgendaReadAcknowledged)
            return Result.Failure<InvitationResponse>(InvitationErrors.AgendaAcknowledgementRequired);

        var invitation = await dbcontext.MeetingInvitations
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure<InvitationResponse>(InvitationErrors.NotFound);

        if (boardAccessService is not null && !boardAccessService.IsCurrentUser(invitation.MemberUserId))
            return Result.Failure<InvitationResponse>(PermissionErrors.Forbidden);

        if (invitation.ExpiresAt < DateTime.UtcNow.AddHours(3))
        {
            invitation.Status = InvitationStatus.Expired;
            await dbcontext.SaveChangesAsync(cancellationToken);
            return Result.Failure<InvitationResponse>(InvitationErrors.Expired);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow.AddHours(3);
        invitation.AgendaReadAcknowledged = true;

        if (request.Note is not null && !string.IsNullOrWhiteSpace(request.Note.Text))
        {
            invitation.Notes.Add(new MeetingNote
            {
                Text = request.Note.Text.Trim(),
                Visibility = request.Note.Visibility,
                MeetingAgendaItemId = request.Note.MeetingAgendaItemId
            });
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        var response = MapInvitation(invitation);
        if (realtimeNotifier is not null)
            await realtimeNotifier.InvitationAcceptedAsync(invitation.BoardMeetingId, response, cancellationToken);

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<InvitationResponse>>> GetMeetingInvitationsAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(meetingId, cancellationToken))
            return Result.Failure<IEnumerable<InvitationResponse>>(PermissionErrors.Forbidden);

        var invitations = await dbcontext.MeetingInvitations
            .AsNoTracking()
            .Where(x => x.BoardMeetingId == meetingId)
            .OrderBy(x => x.MemberUserId)
            .Select(x => MapInvitation(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<InvitationResponse>>(invitations);
    }

    private static InvitationResponse MapInvitation(MeetingInvitation invitation) =>
        new(
            invitation.Id,
            invitation.BoardMeetingId,
            invitation.MemberUserId,
            invitation.Status.ToString(),
            invitation.ExpiresAt,
            invitation.AcceptedAt,
            invitation.AgendaReadAcknowledged);
}
