using Application.Contracts.Voting;
using Application.Service.Voting;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class VotingServiceTests
{
    [Fact]
    public async Task CloseAsync_ApprovesDecisionWhenApproveVotesExceedHalfOfPresentMembers()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var item = await SeedVotingMeetingAsync(dbcontext, acceptedMembers: ["u1", "u2", "u3"]);
        var service = new VotingService(dbcontext);

        var open = await service.OpenAsync(item.Id);
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u1", VoteChoice.Approve, null));
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u2", VoteChoice.Approve, null));
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u3", VoteChoice.Reject, "Reason"));

        var closed = await service.CloseAsync(open.Value.Id);

        Assert.True(closed.IsSuccess);
        Assert.Contains(dbcontext.Decisions, x => x.MeetingAgendaItemId == item.Id && x.Code == "BD-2026-1-001");
    }

    [Fact]
    public async Task CloseAsync_RejectsItemWhenQuorumIsNotMet()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var item = await SeedVotingMeetingAsync(dbcontext, acceptedMembers: ["u1", "u2", "u3"]);
        var service = new VotingService(dbcontext);

        var open = await service.OpenAsync(item.Id);
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u1", VoteChoice.Approve, null));
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u2", VoteChoice.Reject, "Reason"));
        await service.CastVoteAsync(open.Value.Id, new CastVoteRequest("u3", VoteChoice.Abstain, null));

        var closed = await service.CloseAsync(open.Value.Id);

        Assert.True(closed.IsSuccess);
        Assert.Equal(AgendaItemStatus.Rejected, dbcontext.MeetingAgendaItems.Single(x => x.Id == item.Id).Status);
        Assert.Contains("بند مرفوض لعدم اكتمال النصاب", dbcontext.MeetingMinutes.Single().DraftText);
    }

    private static async Task<MeetingAgendaItem> SeedVotingMeetingAsync(Domain.ApplicationDbcontext dbcontext, string[] acceptedMembers)
    {
        var board = new Board { Name = "Board", Code = "BD" };
        var cycle = new BoardCycle
        {
            Board = board,
            CycleNumber = 1,
            ConsecutiveCycleCount = 1,
            StartsAt = new DateTime(2026, 1, 1),
            EndsAt = new DateTime(2026, 12, 31)
        };
        var meeting = new BoardMeeting
        {
            BoardCycle = cycle,
            Title = "Meeting",
            ScheduledAt = new DateTime(2026, 7, 1),
            Status = MeetingStatus.InProgress
        };
        var item = new MeetingAgendaItem
        {
            BoardMeeting = meeting,
            ItemNumber = 1,
            Title = "Decision item",
            RequiresDecision = true
        };

        foreach (var userId in acceptedMembers)
        {
            board.Memberships.Add(new BoardMembership { UserId = userId, HasPaidFees = true });
            meeting.Invitations.Add(new MeetingInvitation
            {
                MemberUserId = userId,
                Status = InvitationStatus.Accepted,
                AgendaReadAcknowledged = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
        }

        dbcontext.MeetingAgendaItems.Add(item);
        await dbcontext.SaveChangesAsync();
        return item;
    }
}
