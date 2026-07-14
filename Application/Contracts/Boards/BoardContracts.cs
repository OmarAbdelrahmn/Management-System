namespace Application.Contracts.Boards;

public record CreateBoardRequest(
    string Name,
    string Code,
    DateTime CycleStartsAt,
    DateTime CycleEndsAt,
    int ConsecutiveCycleCount,
    IEnumerable<CreateBoardMemberRequest> Members);

public record CreateBoardMemberRequest(
    string UserId,
    bool HasPaidFees,
    bool IsSupportingMember,
    decimal CumulativePercentage,
    bool IsChairman,
    bool IsSecretary);

public record UpdateBoardRequest(string Name, string Code);

public record SaveBoardMembersRequest(IEnumerable<CreateBoardMemberRequest> Members);

public record RenewBoardCycleRequest(
    DateTime CycleStartsAt,
    DateTime CycleEndsAt,
    int ConsecutiveCycleCount);

public record BoardResponse(
    int Id,
    string Name,
    string Code,
    string Status,
    BoardCycleResponse CurrentCycle,
    IEnumerable<BoardMemberResponse> Members);

public record BoardCycleResponse(
    int Id,
    int CycleNumber,
    int ConsecutiveCycleCount,
    DateTime StartsAt,
    DateTime EndsAt,
    int NextDecisionSequence);

public record BoardMemberResponse(
    int Id,
    string UserId,
    bool HasPaidFees,
    bool IsSupportingMember,
    decimal CumulativePercentage,
    bool IsChairman,
    bool IsSecretary);
