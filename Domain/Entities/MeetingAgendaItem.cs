using Domain.Auditing;

namespace Domain.Entities;

public class MeetingAgendaItem : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public int ItemNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresDecision { get; set; }
    public AgendaItemStatus Status { get; set; } = AgendaItemStatus.Pending;
    public string? RejectionText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
    public VoteSession? VoteSession { get; set; }
    public Decision? Decision { get; set; }
}
