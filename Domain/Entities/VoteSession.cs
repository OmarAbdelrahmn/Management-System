using Domain.Auditing;

namespace Domain.Entities;

public class VoteSession : IAuditable
{
    public int Id { get; set; }
    public int MeetingAgendaItemId { get; set; }
    public VoteSessionStatus Status { get; set; } = VoteSessionStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? ClosedAt { get; set; }

    public MeetingAgendaItem? MeetingAgendaItem { get; set; }
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
