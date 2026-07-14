using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Voting;
using Application.Service.Permissions;
using Application.Service.Realtime;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Voting;

public class VotingService(
    ApplicationDbcontext dbcontext,
    IBoardAccessService? boardAccessService = null,
    IMeetingRealtimeNotifier? realtimeNotifier = null) : IVotingService
{
    private const string RejectedForQuorumText = "بند مرفوض لعدم تحقق أغلبية الوزن التصويتي للحضور";

    public async Task<Result<VoteSessionResponse>> OpenAsync(int agendaItemId, CancellationToken cancellationToken = default)
    {
        var item = await dbcontext.MeetingAgendaItems
            .Include(x => x.VoteSession)
            .FirstOrDefaultAsync(x => x.Id == agendaItemId, cancellationToken);

        if (item is null)
            return Result.Failure<VoteSessionResponse>(MeetingErrors.AgendaItemNotFound);

        if (!item.RequiresDecision)
            return Result.Failure<VoteSessionResponse>(VotingErrors.AgendaItemDoesNotRequireDecision);

        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(item.BoardMeetingId, cancellationToken))
            return Result.Failure<VoteSessionResponse>(PermissionErrors.Forbidden);

        if (item.VoteSession is not null)
            return Result.Failure<VoteSessionResponse>(VotingErrors.AlreadyOpen);

        var session = new VoteSession { MeetingAgendaItemId = agendaItemId };
        dbcontext.VoteSessions.Add(session);
        await dbcontext.SaveChangesAsync(cancellationToken);

        var result = await GetAsync(session.Id, cancellationToken);
        if (result.IsSuccess && realtimeNotifier is not null)
            await realtimeNotifier.VoteOpenedAsync(item.BoardMeetingId, result.Value, cancellationToken);

        return result;
    }

    public async Task<Result<VoteSessionResponse>> CastVoteAsync(int voteSessionId, CastVoteRequest request, CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionQuery()
            .FirstOrDefaultAsync(x => x.Id == voteSessionId, cancellationToken);

        if (session is null)
            return Result.Failure<VoteSessionResponse>(VotingErrors.NotFound);

        if (session.Status == VoteSessionStatus.Closed)
            return Result.Failure<VoteSessionResponse>(VotingErrors.Closed);

        var meetingId = session.MeetingAgendaItem!.BoardMeetingId;
        if (boardAccessService is not null && !await boardAccessService.CanVoteForMemberAsync(meetingId, request.MemberUserId, cancellationToken))
            return Result.Failure<VoteSessionResponse>(PermissionErrors.Forbidden);

        var invitation = await dbcontext.MeetingInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BoardMeetingId == meetingId &&
                x.MemberUserId == request.MemberUserId &&
                x.Status == InvitationStatus.Accepted &&
                x.AgendaReadAcknowledged,
                cancellationToken);

        if (invitation is null)
            return Result.Failure<VoteSessionResponse>(VotingErrors.MemberNotPresent);

        if (request.Choice == VoteChoice.Reject && string.IsNullOrWhiteSpace(request.RejectionReason))
            return Result.Failure<VoteSessionResponse>(VotingErrors.InvalidRequest);

        var weight = await ResolveVoteWeightAsync(session.MeetingAgendaItem.BoardMeetingId, request.MemberUserId, cancellationToken);
        var vote = session.Votes.FirstOrDefault(x => x.MemberUserId == request.MemberUserId);
        if (vote is null)
        {
            vote = new Vote
            {
                VoteSessionId = voteSessionId,
                MemberUserId = request.MemberUserId
            };
            dbcontext.Votes.Add(vote);
        }

        vote.Choice = request.Choice;
        vote.Weight = weight;
        vote.RejectionReason = request.Choice == VoteChoice.Reject ? request.RejectionReason?.Trim() : null;
        vote.CreatedAt = DateTime.UtcNow.AddHours(3);

        await dbcontext.SaveChangesAsync(cancellationToken);
        var result = await GetAsync(voteSessionId, cancellationToken);
        if (result.IsSuccess && realtimeNotifier is not null)
            await realtimeNotifier.VoteUpdatedAsync(meetingId, result.Value, cancellationToken);

        return result;
    }

    public async Task<Result<VoteSessionResponse>> CloseAsync(int voteSessionId, CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionQuery()
            .FirstOrDefaultAsync(x => x.Id == voteSessionId, cancellationToken);

        if (session is null)
            return Result.Failure<VoteSessionResponse>(VotingErrors.NotFound);

        if (session.Status == VoteSessionStatus.Closed)
            return Result.Failure<VoteSessionResponse>(VotingErrors.Closed);

        var item = session.MeetingAgendaItem!;
        if (boardAccessService is not null && !await boardAccessService.CanManageMeetingAsync(item.BoardMeetingId, cancellationToken))
            return Result.Failure<VoteSessionResponse>(PermissionErrors.Forbidden);

        var meeting = await dbcontext.BoardMeetings
            .Include(x => x.BoardCycle)
            .ThenInclude(x => x!.Board)
            .Include(x => x.Minute)
            .FirstAsync(x => x.Id == item.BoardMeetingId, cancellationToken);

        var attendance = await GetPresentAttendanceAsync(item.BoardMeetingId, cancellationToken);

        session.Status = VoteSessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow.AddHours(3);

        if (session.Votes.Where(x => x.Choice == VoteChoice.Approve).Sum(x => x.Weight) > attendance.PresentWeight / 2m)
        {
            item.Status = AgendaItemStatus.DecisionApproved;
            var cycle = meeting.BoardCycle!;
            var sequence = cycle.NextDecisionSequence;
            cycle.NextDecisionSequence += 1;

            dbcontext.Decisions.Add(new Decision
            {
                MeetingAgendaItemId = item.Id,
                Sequence = sequence,
                Code = $"{cycle.Board!.Code}-{meeting.ScheduledAt:yyyy}-{cycle.CycleNumber}-{sequence:000}"
            });
            meeting.Status = MeetingStatus.WaitingChairmanApproval;
        }
        else
        {
            item.Status = AgendaItemStatus.Rejected;
            item.RejectionText = RejectedForQuorumText;
            var minute = meeting.Minute ?? new MeetingMinute { BoardMeetingId = meeting.Id };
            minute.DraftText = AppendLine(minute.DraftText, RejectedForQuorumText);
            if (meeting.Minute is null)
                dbcontext.MeetingMinutes.Add(minute);
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        var result = await GetAsync(voteSessionId, cancellationToken);
        if (result.IsSuccess && realtimeNotifier is not null)
            await realtimeNotifier.VoteClosedAsync(item.BoardMeetingId, result.Value, cancellationToken);

        return result;
    }

    public async Task<Result<VoteSessionResponse>> GetAsync(int voteSessionId, CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == voteSessionId, cancellationToken);

        if (session is null)
            return Result.Failure<VoteSessionResponse>(VotingErrors.NotFound);

        if (boardAccessService is not null && !await boardAccessService.CanAccessMeetingAsync(session.MeetingAgendaItem!.BoardMeetingId, cancellationToken))
            return Result.Failure<VoteSessionResponse>(PermissionErrors.Forbidden);

        var attendance = await GetPresentAttendanceAsync(session.MeetingAgendaItem!.BoardMeetingId, cancellationToken);

        return Result.Success(MapSession(session, attendance.PresentMembers, attendance.PresentWeight));
    }

    private IQueryable<VoteSession> LoadSessionQuery() =>
        dbcontext.VoteSessions
            .Include(x => x.Votes)
            .Include(x => x.MeetingAgendaItem);

    private async Task<decimal> ResolveVoteWeightAsync(int meetingId, string memberUserId, CancellationToken cancellationToken)
    {
        var boardId = await dbcontext.BoardMeetings
            .AsNoTracking()
            .Where(x => x.Id == meetingId)
            .Select(x => x.BoardCycle!.BoardId)
            .FirstAsync(cancellationToken);

        var membership = await dbcontext.BoardMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BoardId == boardId && x.UserId == memberUserId && x.IsActive, cancellationToken);

        return membership?.IsSupportingMember == true
            ? membership.CumulativePercentage
            : 1;
    }

    private async Task<(int PresentMembers, decimal PresentWeight)> GetPresentAttendanceAsync(int meetingId, CancellationToken cancellationToken)
    {
        var presentUserIds = await dbcontext.MeetingInvitations
            .AsNoTracking()
            .Where(x => x.BoardMeetingId == meetingId && x.Status == InvitationStatus.Accepted && x.AgendaReadAcknowledged)
            .Select(x => x.MemberUserId)
            .ToListAsync(cancellationToken);

        var boardId = await dbcontext.BoardMeetings.AsNoTracking()
            .Where(x => x.Id == meetingId)
            .Select(x => x.BoardCycle!.BoardId)
            .FirstAsync(cancellationToken);

        var memberships = await dbcontext.BoardMemberships.AsNoTracking()
            .Where(x => x.BoardId == boardId && x.IsActive && presentUserIds.Contains(x.UserId))
            .Select(x => new { x.UserId, x.IsSupportingMember, x.CumulativePercentage })
            .ToListAsync(cancellationToken);

        return (memberships.Count, memberships.Sum(x => x.IsSupportingMember ? x.CumulativePercentage : 1m));
    }

    private static VoteSessionResponse MapSession(VoteSession session, int presentMembers, decimal presentWeight)
    {
        var votes = session.Votes.OrderBy(x => x.MemberUserId).ToList();

        return new VoteSessionResponse(
            session.Id,
            session.MeetingAgendaItemId,
            session.Status.ToString(),
            session.OpenedAt,
            session.ClosedAt,
            new VoteSummaryResponse(
                presentMembers,
                presentWeight,
                votes.Count(x => x.Choice == VoteChoice.Approve),
                votes.Where(x => x.Choice == VoteChoice.Approve).Sum(x => x.Weight),
                votes.Count(x => x.Choice == VoteChoice.Reject),
                votes.Where(x => x.Choice == VoteChoice.Reject).Sum(x => x.Weight),
                votes.Count(x => x.Choice == VoteChoice.Abstain),
                votes.Where(x => x.Choice == VoteChoice.Abstain).Sum(x => x.Weight),
                votes.Select(x => new VoteResponse(x.MemberUserId, x.Choice, x.Weight, x.RejectionReason))));
    }

    private static string AppendLine(string current, string line) =>
        string.IsNullOrWhiteSpace(current) ? line : $"{current}{Environment.NewLine}{line}";
}
