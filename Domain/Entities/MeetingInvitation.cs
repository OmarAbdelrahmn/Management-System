using Domain.Auditing;

namespace Domain.Entities;

public class MeetingInvitation : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string MemberUserId { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public bool AgendaReadAcknowledged { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
    public ICollection<MeetingNote> Notes { get; set; } = new List<MeetingNote>();
}
