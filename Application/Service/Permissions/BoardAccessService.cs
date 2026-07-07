using Domain;
using Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.Permissions;

public class BoardAccessService(ApplicationDbcontext dbcontext, ICurrentUserContext currentUserContext) : IBoardAccessService
{
    public async Task<IReadOnlyCollection<int>> GetAccessibleBoardIdsAsync(CancellationToken cancellationToken = default)
    {
        if (IsAdmin())
        {
            return await dbcontext.Boards
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        return await dbcontext.BoardMemberships
            .AsNoTracking()
            .Where(x => x.UserId == currentUserContext.UserId && x.IsActive)
            .Select(x => x.BoardId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CanAccessBoardAsync(int boardId, CancellationToken cancellationToken = default)
    {
        if (IsAdmin())
            return true;

        return await dbcontext.BoardMemberships
            .AsNoTracking()
            .AnyAsync(x => x.BoardId == boardId && x.UserId == currentUserContext.UserId && x.IsActive, cancellationToken);
    }

    public async Task<bool> CanAccessMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var boardId = await GetMeetingBoardIdAsync(meetingId, cancellationToken);
        return boardId is not null && await CanAccessBoardAsync(boardId.Value, cancellationToken);
    }

    public async Task<bool> CanManageBoardAsync(int boardId, CancellationToken cancellationToken = default)
    {
        if (IsAdmin())
            return true;

        return await dbcontext.BoardMemberships
            .AsNoTracking()
            .AnyAsync(x => x.BoardId == boardId && x.UserId == currentUserContext.UserId && x.IsActive && x.IsSecretary, cancellationToken);
    }

    public async Task<bool> CanChairBoardAsync(int boardId, CancellationToken cancellationToken = default)
    {
        if (IsAdmin())
            return true;

        return await dbcontext.BoardMemberships
            .AsNoTracking()
            .AnyAsync(x => x.BoardId == boardId && x.UserId == currentUserContext.UserId && x.IsActive && x.IsChairman, cancellationToken);
    }

    public async Task<bool> CanManageMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var boardId = await GetMeetingBoardIdAsync(meetingId, cancellationToken);
        return boardId is not null && await CanManageBoardAsync(boardId.Value, cancellationToken);
    }

    public async Task<bool> CanChairMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var boardId = await GetMeetingBoardIdAsync(meetingId, cancellationToken);
        return boardId is not null && await CanChairBoardAsync(boardId.Value, cancellationToken);
    }

    public async Task<bool> CanVoteForMemberAsync(int meetingId, string memberUserId, CancellationToken cancellationToken = default)
    {
        if (!IsCurrentUser(memberUserId) && !IsAdmin())
            return false;

        var boardId = await GetMeetingBoardIdAsync(meetingId, cancellationToken);
        if (boardId is null)
            return false;

        return await dbcontext.BoardMemberships
            .AsNoTracking()
            .AnyAsync(x => x.BoardId == boardId && x.UserId == memberUserId && x.IsActive, cancellationToken);
    }

    public bool IsCurrentUser(string userId) =>
        string.IsNullOrWhiteSpace(currentUserContext.UserId) || currentUserContext.UserId == userId || IsAdmin();

    private bool IsAdmin() => currentUserContext.Roles.Contains("Admin");

    private Task<int?> GetMeetingBoardIdAsync(int meetingId, CancellationToken cancellationToken) =>
        dbcontext.BoardMeetings
            .AsNoTracking()
            .Where(x => x.Id == meetingId)
            .Select(x => (int?)x.BoardCycle!.BoardId)
            .FirstOrDefaultAsync(cancellationToken);
}
