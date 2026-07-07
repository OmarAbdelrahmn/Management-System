using Domain.Auditing;

namespace Domain.Entities;

public class BoardMeeting : IAuditable
{
    public int Id { get; set; }
    public int BoardCycleId { get; set; }
    public int Serial { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int AcceptanceDeadlineDays { get; set; } = 15;
    public MeetingType Type { get; set; } = MeetingType.General;
    public string? Category { get; set; }
    public MeetingImportance Importance { get; set; } = MeetingImportance.Normal;
    public bool HasVoting { get; set; } = true;
    public bool IsOnline { get; set; }
    public string? Platform { get; set; }
    public string? Location { get; set; }
    public bool ReminderEnabled { get; set; }
    public DateTime? ReminderAt { get; set; }
    public int? DurationMinutes { get; set; }
    public MeetingRepeatMode RepeatMode { get; set; } = MeetingRepeatMode.None;
    public DateTime? RepeatUntil { get; set; }
    public decimal MinimumAttendancePercentage { get; set; } = 100;
    public int? AvailableSeats { get; set; }
    public string? Notes { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Draft;
    public DateTime? InvitationsSentAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardCycle? BoardCycle { get; set; }
    public ICollection<MeetingAgendaItem> AgendaItems { get; set; } = new List<MeetingAgendaItem>();
    public ICollection<MeetingInvitation> Invitations { get; set; } = new List<MeetingInvitation>();
    public ICollection<MeetingManager> Managers { get; set; } = new List<MeetingManager>();
    public ICollection<MeetingCandidate> Candidates { get; set; } = new List<MeetingCandidate>();
    public ICollection<MeetingGuest> Guests { get; set; } = new List<MeetingGuest>();
    public ICollection<MeetingAttachment> Attachments { get; set; } = new List<MeetingAttachment>();
    public ICollection<MeetingImage> Images { get; set; } = new List<MeetingImage>();
    public ICollection<MeetingApproval> Approvals { get; set; } = new List<MeetingApproval>();
    public ICollection<MeetingRepeatDraft> RepeatDrafts { get; set; } = new List<MeetingRepeatDraft>();
    public MeetingMinute? Minute { get; set; }
}
