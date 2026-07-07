using Domain.Auditing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

public class AuditingTests
{
    [Fact]
    public async Task SaveChangesAsync_SetsCreatedAuditValues()
    {
        await using var dbcontext = CreateDbContext("creator-user");

        var board = new Board { Name = "Board", Code = "BD" };
        dbcontext.Boards.Add(board);
        await dbcontext.SaveChangesAsync();

        Assert.NotEqual(default, board.CreatedAt);
        Assert.Equal("creator-user", board.CreatedByUserId);
        Assert.Null(board.UpdatedAt);
        Assert.Null(board.UpdatedByUserId);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsUpdatedAuditValuesWithoutChangingCreatedValues()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var dbcontext = CreateDbContext("creator-user", databaseName);

        var board = new Board { Name = "Board", Code = "BD" };
        dbcontext.Boards.Add(board);
        await dbcontext.SaveChangesAsync();
        var createdAt = board.CreatedAt;
        var createdBy = board.CreatedByUserId;

        dbcontext.ChangeTracker.Clear();

        await using var updateContext = CreateDbContext("updater-user", databaseName);
        var existing = await updateContext.Boards.SingleAsync(x => x.Code == "BD");
        existing.Name = "Updated Board";
        await updateContext.SaveChangesAsync();

        Assert.Equal(createdAt, existing.CreatedAt);
        Assert.Equal(createdBy, existing.CreatedByUserId);
        Assert.NotNull(existing.UpdatedAt);
        Assert.Equal("updater-user", existing.UpdatedByUserId);
    }

    private static Domain.ApplicationDbcontext CreateDbContext(string userId, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<Domain.ApplicationDbcontext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new Domain.ApplicationDbcontext(options, new TestCurrentUserContext(userId));
    }

    private sealed class TestCurrentUserContext(string userId) : ICurrentUserContext
    {
        public string? UserId { get; } = userId;
        public IReadOnlyCollection<string> Roles { get; } = [];
    }
}
