using Application.Abstraction;
using Application.Contracts.Voting;

namespace Application.Service.Voting;

public interface IVotingService
{
    Task<Result<VoteSessionResponse>> OpenAsync(int agendaItemId, CancellationToken cancellationToken = default);
    Task<Result<VoteSessionResponse>> CastVoteAsync(int voteSessionId, CastVoteRequest request, CancellationToken cancellationToken = default);
    Task<Result<VoteSessionResponse>> CloseAsync(int voteSessionId, CancellationToken cancellationToken = default);
    Task<Result<VoteSessionResponse>> GetAsync(int voteSessionId, CancellationToken cancellationToken = default);
}
