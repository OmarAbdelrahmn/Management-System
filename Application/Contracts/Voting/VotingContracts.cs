using Domain.Entities;

namespace Application.Contracts.Voting;

public record CastVoteRequest(
    string MemberUserId,
    VoteChoice Choice,
    string? RejectionReason);

public record VoteSessionResponse(
    int Id,
    int MeetingAgendaItemId,
    string Status,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    VoteSummaryResponse Summary);

public record VoteSummaryResponse(
    int PresentMembers,
    int ApproveCount,
    decimal ApproveWeight,
    int RejectCount,
    decimal RejectWeight,
    int AbstainCount,
    decimal AbstainWeight,
    IEnumerable<VoteResponse> Votes);

public record VoteResponse(
    string MemberUserId,
    VoteChoice Choice,
    decimal Weight,
    string? RejectionReason);
