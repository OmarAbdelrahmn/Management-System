using Domain.Auditing;

namespace Domain.Entities;

public class MeetingMinute : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string DraftText { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PdfPath { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}
