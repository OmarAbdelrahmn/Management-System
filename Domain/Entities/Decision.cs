using Domain.Auditing;

namespace Domain.Entities;

public class Decision : IAuditable
{
    public int Id { get; set; }
    public int MeetingAgendaItemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public DecisionStatus Status { get; set; } = DecisionStatus.WaitingChairmanSignature;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? SignedByUserId { get; set; }

    public MeetingAgendaItem? MeetingAgendaItem { get; set; }
}
