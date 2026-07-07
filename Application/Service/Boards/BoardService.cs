using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Boards;
using Application.Service.Permissions;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Boards;

public class BoardService(ApplicationDbcontext dbcontext, IBoardAccessService? boardAccessService = null) : IBoardService
{
    public async Task<Result<IEnumerable<BoardResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Board> query = dbcontext.Boards
            .AsNoTracking()
            .Include(x => x.Cycles)
            .Include(x => x.Memberships);

        if (boardAccessService is not null)
        {
            var accessibleBoardIds = await boardAccessService.GetAccessibleBoardIdsAsync(cancellationToken);
            query = query.Where(x => accessibleBoardIds.Contains(x.Id));
        }

        var boards = await query
            .OrderBy(x => x.Name)
            .Select(x => MapBoard(x))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BoardResponse>>(boards);
    }

    public async Task<Result<BoardResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var board = await dbcontext.Boards
            .AsNoTracking()
            .Include(x => x.Cycles)
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return board is null
            ? Result.Failure<BoardResponse>(BoardErrors.NotFound)
            : boardAccessService is not null && !await boardAccessService.CanAccessBoardAsync(board.Id, cancellationToken)
                ? Result.Failure<BoardResponse>(PermissionErrors.Forbidden)
                : Result.Success(MapBoard(board));
    }

    public async Task<Result<BoardResponse>> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CycleEndsAt <= request.CycleStartsAt || request.CycleEndsAt > request.CycleStartsAt.AddYears(1))
            return Result.Failure<BoardResponse>(BoardErrors.CycleTooLong);

        if (request.ConsecutiveCycleCount is < 1 or > 4)
            return Result.Failure<BoardResponse>(BoardErrors.TooManyConsecutiveCycles);

        var board = new Board
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Status = BoardStatus.Active,
            Cycles =
            {
                new BoardCycle
                {
                    CycleNumber = 1,
                    ConsecutiveCycleCount = request.ConsecutiveCycleCount,
                    StartsAt = request.CycleStartsAt,
                    EndsAt = request.CycleEndsAt
                }
            }
        };

        foreach (var member in request.Members)
        {
            board.Memberships.Add(new BoardMembership
            {
                UserId = member.UserId,
                HasPaidFees = member.HasPaidFees,
                IsSupportingMember = member.IsSupportingMember,
                CumulativePercentage = member.IsSupportingMember ? member.CumulativePercentage : 0,
                IsChairman = member.IsChairman,
                IsSecretary = member.IsSecretary
            });
        }

        dbcontext.Boards.Add(board);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapBoard(board));
    }

    public async Task<Result<int>> CloseExpiredBoardsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var expiredBoards = await dbcontext.Boards
            .Include(x => x.Cycles)
            .Where(x => x.Status == BoardStatus.Active && x.Cycles.Any(c => c.EndsAt < now))
            .ToListAsync(cancellationToken);

        foreach (var board in expiredBoards)
        {
            var latestCycle = board.Cycles.OrderByDescending(x => x.CycleNumber).FirstOrDefault();
            if (latestCycle is not null && latestCycle.EndsAt < now)
            {
                board.Status = BoardStatus.Closed;
            }
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(expiredBoards.Count(x => x.Status == BoardStatus.Closed));
    }

    private static BoardResponse MapBoard(Board board)
    {
        var currentCycle = board.Cycles.OrderByDescending(x => x.CycleNumber).First();

        return new BoardResponse(
            board.Id,
            board.Name,
            board.Code,
            board.Status.ToString(),
            new BoardCycleResponse(
                currentCycle.Id,
                currentCycle.CycleNumber,
                currentCycle.ConsecutiveCycleCount,
                currentCycle.StartsAt,
                currentCycle.EndsAt,
                currentCycle.NextDecisionSequence),
            board.Memberships
                .OrderBy(x => x.Id)
                .Select(x => new BoardMemberResponse(
                    x.Id,
                    x.UserId,
                    x.HasPaidFees,
                    x.IsSupportingMember,
                    x.CumulativePercentage,
                    x.IsChairman,
                    x.IsSecretary)));
    }
}
