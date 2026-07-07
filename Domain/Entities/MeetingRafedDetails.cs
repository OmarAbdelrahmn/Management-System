using Domain.Auditing;

namespace Domain.Entities;

public class MeetingManager : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingCandidate : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingGuest : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingAttachment : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingImage : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingApproval : IAuditable
{
    public int Id { get; set; }
    public int BoardMeetingId { get; set; }
    public string ApproverUserId { get; set; } = string.Empty;
    public MeetingApprovalStatus Status { get; set; } = MeetingApprovalStatus.Pending;
    public string? Comments { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? BoardMeeting { get; set; }
}

public class MeetingRepeatDraft : IAuditable
{
    public int Id { get; set; }
    public int SourceBoardMeetingId { get; set; }
    public int? CreatedBoardMeetingId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public BoardMeeting? SourceBoardMeeting { get; set; }
    public BoardMeeting? CreatedBoardMeeting { get; set; }
}
