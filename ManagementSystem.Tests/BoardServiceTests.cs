using Application.Contracts.Boards;
using Application.Service.Boards;

namespace ManagementSystem.Tests;

public class BoardServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsCycleLongerThanOneYear()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BoardService(dbcontext);

        var result = await service.CreateAsync(new CreateBoardRequest(
            "Board",
            "BD",
            new DateTime(2026, 1, 1),
            new DateTime(2027, 1, 2),
            1,
            []));

        Assert.True(result.IsFailure);
        Assert.Equal("Board.CycleTooLong", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_RejectsMoreThanFourConsecutiveCycles()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new BoardService(dbcontext);

        var result = await service.CreateAsync(new CreateBoardRequest(
            "Board",
            "BD",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            5,
            []));

        Assert.True(result.IsFailure);
        Assert.Equal("Board.TooManyConsecutiveCycles", result.Error.Code);
    }
}
