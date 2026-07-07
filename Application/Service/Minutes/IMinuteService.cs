using Application.Abstraction;
using Application.Contracts.Minutes;

namespace Application.Service.Minutes;

public interface IMinuteService
{
    Task<Result<IEnumerable<MinuteResponse>>> GetArchiveAsync(CancellationToken cancellationToken = default);
    Task<Result<MinuteResponse>> GetAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<Result<MinuteResponse>> SignAndPublishAsync(int meetingId, SignMinuteRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelApprovalAsync(int meetingId, string actorUserId, CancellationToken cancellationToken = default);
    Task<Result<byte[]>> GeneratePdfAsync(int meetingId, CancellationToken cancellationToken = default);
}
