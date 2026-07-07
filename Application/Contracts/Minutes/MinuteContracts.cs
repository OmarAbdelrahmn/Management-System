namespace Application.Contracts.Minutes;

public record DecisionResponse(
    int Id,
    int MeetingAgendaItemId,
    string Code,
    int Sequence,
    string Status,
    DateTime CreatedAt,
    DateTime? SignedAt,
    string? SignedByUserId);

public record MinuteResponse(
    int Id,
    int BoardMeetingId,
    string DraftText,
    bool IsReadOnly,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    IEnumerable<DecisionResponse> Decisions);

public record SignMinuteRequest(string ChairmanUserId);
