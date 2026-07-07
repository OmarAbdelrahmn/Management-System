using Application.Service.Permissions;
using Domain.Auditing;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class PermissionServiceTests
{
    [Fact]
    public async Task CanManageMeetingAsync_ReturnsTrueForBoardSecretary()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var meeting = SeedMeeting(dbcontext, secretaryUserId: "secretary", chairmanUserId: "chairman", memberUserId: "member");
        await dbcontext.SaveChangesAsync();

        var service = new BoardAccessService(dbcontext, new TestCurrentUserContext("secretary"));

        Assert.True(await service.CanManageMeetingAsync(meeting.Id));
    }

    [Fact]
    public async Task CanManageMeetingAsync_ReturnsFalseForRegularBoardMember()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var meeting = SeedMeeting(dbcontext, secretaryUserId: "secretary", chairmanUserId: "chairman", memberUserId: "member");
        await dbcontext.SaveChangesAsync();

        var service = new BoardAccessService(dbcontext, new TestCurrentUserContext("member"));

        Assert.False(await service.CanManageMeetingAsync(meeting.Id));
    }

    private static BoardMeeting SeedMeeting(Domain.ApplicationDbcontext dbcontext, string secretaryUserId, string chairmanUserId, string memberUserId)
    {
        var board = new Board
        {
            Name = "Board",
            Code = "BD",
            Memberships =
            {
                new BoardMembership { UserId = secretaryUserId, IsSecretary = true, IsActive = true },
                new BoardMembership { UserId = chairmanUserId, IsChairman = true, IsActive = true },
                new BoardMembership { UserId = memberUserId, IsActive = true }
            }
        };
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
            ScheduledAt = new DateTime(2026, 7, 1)
        };

        dbcontext.BoardMeetings.Add(meeting);
        return meeting;
    }

    private sealed class TestCurrentUserContext(string userId, IReadOnlyCollection<string>? roles = null) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = roles ?? [];
    }
}
