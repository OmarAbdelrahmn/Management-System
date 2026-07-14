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
        var members = request.Members?.ToList() ?? [];
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code) || members.Count == 0 ||
            members.Select(x => x.UserId).Distinct().Count() != members.Count ||
            members.Count(x => x.IsChairman) != 1 || members.Count(x => x.IsSecretary) != 1 ||
            members.Any(x => string.IsNullOrWhiteSpace(x.UserId) || x.CumulativePercentage < 0))
            return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);

        if (request.CycleEndsAt <= request.CycleStartsAt || request.CycleEndsAt > request.CycleStartsAt.AddYears(1))
            return Result.Failure<BoardResponse>(BoardErrors.CycleTooLong);

        if (request.ConsecutiveCycleCount is < 1 or > 4)
            return Result.Failure<BoardResponse>(BoardErrors.TooManyConsecutiveCycles);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbcontext.Boards.AnyAsync(x => x.Code == code, cancellationToken))
            return Result.Failure<BoardResponse>(BoardErrors.DuplicateCode);

        var memberIds = members.Select(x => x.UserId).ToList();
        if (await dbcontext.Users.CountAsync(x => memberIds.Contains(x.Id) && x.IsActive, cancellationToken) != memberIds.Count)
            return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);

        var board = new Board
        {
            Name = request.Name.Trim(),
            Code = code,
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

        foreach (var member in members)
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

    public async Task<Result<BoardResponse>> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbcontext.Boards.Include(x => x.Cycles).Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (board is null) return Result.Failure<BoardResponse>(BoardErrors.NotFound);
        if (boardAccessService is not null && !await boardAccessService.CanManageBoardAsync(id, cancellationToken))
            return Result.Failure<BoardResponse>(PermissionErrors.Forbidden);
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
            return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbcontext.Boards.AnyAsync(x => x.Id != id && x.Code == code, cancellationToken))
            return Result.Failure<BoardResponse>(BoardErrors.DuplicateCode);
        board.Name = request.Name.Trim();
        board.Code = code;
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapBoard(board));
    }

    public async Task<Result<BoardResponse>> SaveMembersAsync(int id, SaveBoardMembersRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbcontext.Boards.Include(x => x.Cycles).Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (board is null) return Result.Failure<BoardResponse>(BoardErrors.NotFound);
        if (board.Status == BoardStatus.Closed) return Result.Failure<BoardResponse>(BoardErrors.Closed);
        if (boardAccessService is not null && !await boardAccessService.CanManageBoardAsync(id, cancellationToken))
            return Result.Failure<BoardResponse>(PermissionErrors.Forbidden);

        var members = request.Members?.ToList() ?? [];
        if (!AreValidMembers(members)) return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);
        var userIds = members.Select(x => x.UserId).ToList();
        if (await dbcontext.Users.CountAsync(x => userIds.Contains(x.Id) && x.IsActive, cancellationToken) != userIds.Count)
            return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);

        dbcontext.BoardMemberships.RemoveRange(board.Memberships);
        board.Memberships.Clear();
        foreach (var member in members)
            board.Memberships.Add(MapMembership(member));
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapBoard(board));
    }

    public async Task<Result<BoardResponse>> RenewCycleAsync(int id, RenewBoardCycleRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbcontext.Boards.Include(x => x.Cycles).Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (board is null) return Result.Failure<BoardResponse>(BoardErrors.NotFound);
        if (boardAccessService is not null && !await boardAccessService.CanManageBoardAsync(id, cancellationToken))
            return Result.Failure<BoardResponse>(PermissionErrors.Forbidden);
        if (request.CycleEndsAt <= request.CycleStartsAt || request.CycleEndsAt > request.CycleStartsAt.AddYears(1))
            return Result.Failure<BoardResponse>(BoardErrors.CycleTooLong);
        if (request.ConsecutiveCycleCount is < 1 or > 4)
            return Result.Failure<BoardResponse>(BoardErrors.TooManyConsecutiveCycles);

        var latest = board.Cycles.OrderByDescending(x => x.CycleNumber).First();
        if (request.CycleStartsAt.Date <= latest.EndsAt.Date)
            return Result.Failure<BoardResponse>(BoardErrors.InvalidRequest);
        board.Cycles.Add(new BoardCycle { CycleNumber = latest.CycleNumber + 1, ConsecutiveCycleCount = request.ConsecutiveCycleCount, StartsAt = request.CycleStartsAt, EndsAt = request.CycleEndsAt });
        board.Status = BoardStatus.Active;
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

    private static bool AreValidMembers(List<CreateBoardMemberRequest> members) =>
        members.Count > 0 && members.Select(x => x.UserId).Distinct().Count() == members.Count &&
        members.Count(x => x.IsChairman) == 1 && members.Count(x => x.IsSecretary) == 1 &&
        members.All(x => !string.IsNullOrWhiteSpace(x.UserId) && x.CumulativePercentage >= 0 && (!x.IsSupportingMember || x.CumulativePercentage > 0));

    private static BoardMembership MapMembership(CreateBoardMemberRequest member) => new()
    {
        UserId = member.UserId, HasPaidFees = member.HasPaidFees, IsSupportingMember = member.IsSupportingMember,
        CumulativePercentage = member.IsSupportingMember ? member.CumulativePercentage : 0,
        IsChairman = member.IsChairman, IsSecretary = member.IsSecretary
    };
}
