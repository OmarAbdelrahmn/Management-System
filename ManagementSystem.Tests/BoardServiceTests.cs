using Application.Contracts.Boards;
using Application.Service.Boards;
using Domain.Identity;

namespace ManagementSystem.Tests;

public class BoardServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsCycleLongerThanOneYear()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BoardService(dbcontext);
        var members = await CreateValidMembersAsync(dbcontext);

        var result = await service.CreateAsync(new CreateBoardRequest(
            "Board",
            "BD",
            new DateTime(2026, 1, 1),
            new DateTime(2027, 1, 2),
            1,
            members));

        Assert.True(result.IsFailure);
        Assert.Equal("Board.CycleTooLong", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_RejectsMoreThanFourConsecutiveCycles()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BoardService(dbcontext);
        var members = await CreateValidMembersAsync(dbcontext);

        var result = await service.CreateAsync(new CreateBoardRequest(
            "Board",
            "BD",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            5,
            members));

        Assert.True(result.IsFailure);
        Assert.Equal("Board.TooManyConsecutiveCycles", result.Error.Code);
    }

    private static async Task<CreateBoardMemberRequest[]> CreateValidMembersAsync(Domain.ApplicationDbcontext dbcontext)
    {
        var users = new[]
        {
            new ApplicationUser { Id = "board-user-1", UserName = "board-user-1", Email = "board-user-1@example.test", IsActive = true },
            new ApplicationUser { Id = "board-user-2", UserName = "board-user-2", Email = "board-user-2@example.test", IsActive = true }
        };
        dbcontext.Users.AddRange(users);
        await dbcontext.SaveChangesAsync();
        return
        [
            new CreateBoardMemberRequest(users[0].Id, true, false, 0, true, false),
            new CreateBoardMemberRequest(users[1].Id, true, false, 0, false, true)
        ];
    }
}
