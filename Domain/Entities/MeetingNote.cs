using Domain.Auditing;

namespace Domain.Entities;

public class MeetingNote : IAuditable
{
    public int Id { get; set; }
    public int MeetingInvitationId { get; set; }
    public int? MeetingAgendaItemId { get; set; }
    public string Text { get; set; } = string.Empty;
    public MeetingNoteVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public MeetingInvitation? MeetingInvitation { get; set; }
    public MeetingAgendaItem? MeetingAgendaItem { get; set; }
}
