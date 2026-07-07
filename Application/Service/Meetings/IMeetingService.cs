using Application.Abstraction;
using Application.Contracts.Meetings;

namespace Application.Service.Meetings;

public interface IMeetingService
{
    Task<Result<MeetingResponse>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MeetingListItemResponse>>> GetScheduledAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MeetingCalendarItemResponse>>> GetCalendarAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MeetingRepeatDraftResponse>>> GetRepeatedDraftsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MeetingListItemResponse>>> GetArchiveAsync(string? type, CancellationToken cancellationToken = default);
    Task<Result<MeetingResponse>> CreateAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default);
    Task<Result<AgendaItemResponse>> AddAgendaItemAsync(int meetingId, AddAgendaItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> SubmitForApprovalAsync(int meetingId, SubmitMeetingApprovalRequest request, CancellationToken cancellationToken = default);
    Task<Result> ApproveAsync(int meetingId, DecideMeetingApprovalRequest request, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(int meetingId, DecideMeetingApprovalRequest request, CancellationToken cancellationToken = default);
    Task<Result> StartAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<Result> FinishAsync(int meetingId, CancellationToken cancellationToken = default);
}
