namespace Application.Service.Permissions;

public interface IBoardAccessService
{
    Task<IReadOnlyCollection<int>> GetAccessibleBoardIdsAsync(CancellationToken cancellationToken = default);
    Task<bool> CanAccessBoardAsync(int boardId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessMeetingAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<bool> CanManageBoardAsync(int boardId, CancellationToken cancellationToken = default);
    Task<bool> CanChairBoardAsync(int boardId, CancellationToken cancellationToken = default);
    Task<bool> CanManageMeetingAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<bool> CanChairMeetingAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<bool> CanVoteForMemberAsync(int meetingId, string memberUserId, CancellationToken cancellationToken = default);
    bool IsCurrentUser(string userId);
}
