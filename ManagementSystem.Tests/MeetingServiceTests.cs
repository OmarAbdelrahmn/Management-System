using Application.Contracts.Meetings;
using Application.Service.Meetings;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class MeetingServiceTests
{
    [Fact]
    public async Task CreateAsync_UsesFifteenDaysAsDefaultAcceptanceDeadline()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var board = new Board { Name = "Board", Code = "BD" };
        var cycle = new BoardCycle
        {
            Board = board,
            CycleNumber = 1,
            ConsecutiveCycleCount = 1,
            StartsAt = new DateTime(2026, 1, 1),
            EndsAt = new DateTime(2026, 12, 31)
        };
        dbcontext.BoardCycles.Add(cycle);
        await dbcontext.SaveChangesAsync();

        var service = new MeetingService(dbcontext);
        var result = await service.CreateAsync(new CreateMeetingRequest(
            cycle.Id,
            "Meeting",
            new DateTime(2026, 7, 1),
            null));

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value.AcceptanceDeadlineDays);
    }
}
