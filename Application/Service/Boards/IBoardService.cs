using Application.Abstraction;
using Application.Contracts.Boards;

namespace Application.Service.Boards;

public interface IBoardService
{
    Task<Result<IEnumerable<BoardResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<BoardResponse>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<BoardResponse>> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken = default);
    Task<Result<BoardResponse>> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken = default);
    Task<Result<BoardResponse>> SaveMembersAsync(int id, SaveBoardMembersRequest request, CancellationToken cancellationToken = default);
    Task<Result<BoardResponse>> RenewCycleAsync(int id, RenewBoardCycleRequest request, CancellationToken cancellationToken = default);
    Task<Result<int>> CloseExpiredBoardsAsync(DateTime now, CancellationToken cancellationToken = default);
}
