using Domain.Auditing;

namespace Domain.Entities;

public enum VolunteerUserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}

public enum VolunteerRequestSource
{
    Internal = 0,
    External = 1
}

public enum VolunteerRequestStatus
{
    Submitted = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4
}

public enum VolunteerOpportunityStatus
{
    Draft = 0,
    Open = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum VolunteerTaskStatus
{
    Open = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum VolunteerAttendanceStatus
{
    Present = 0,
    Absent = 1,
    Excused = 2
}

public class VolunteerUser : IAuditable
{
    public int Id { get; set; }
    public string VolunteerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Skills { get; set; }
    public VolunteerUserStatus Status { get; set; } = VolunteerUserStatus.Active;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<VolunteerRequest> Requests { get; set; } = new List<VolunteerRequest>();
    public ICollection<VolunteerAttendanceRecord> AttendanceRecords { get; set; } = new List<VolunteerAttendanceRecord>();
}

public class VolunteerRequest : IAuditable
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public VolunteerRequestSource Source { get; set; } = VolunteerRequestSource.Internal;
    public string ApplicantName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? OpportunityTitle { get; set; }
    public VolunteerRequestStatus Status { get; set; } = VolunteerRequestStatus.Submitted;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? DecisionNote { get; set; }
    public string? Notes { get; set; }
    public int? VolunteerUserId { get; set; }
    public int? VolunteerOpportunityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public VolunteerUser? VolunteerUser { get; set; }
    public VolunteerOpportunity? VolunteerOpportunity { get; set; }
}

public class VolunteerOpportunity : IAuditable
{
    public int Id { get; set; }
    public string OpportunityNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Department { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? EndDate { get; set; }
    public int Seats { get; set; }
    public VolunteerOpportunityStatus Status { get; set; } = VolunteerOpportunityStatus.Open;
    public string? ProcedureNotes { get; set; }
    public string? ReportSummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<VolunteerRequest> Requests { get; set; } = new List<VolunteerRequest>();
    public ICollection<VolunteerOpportunityTask> Tasks { get; set; } = new List<VolunteerOpportunityTask>();
    public ICollection<VolunteerAttendanceRecord> AttendanceRecords { get; set; } = new List<VolunteerAttendanceRecord>();
}

public class VolunteerOpportunityTask : IAuditable
{
    public int Id { get; set; }
    public int VolunteerOpportunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public VolunteerTaskStatus Status { get; set; } = VolunteerTaskStatus.Open;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public VolunteerOpportunity? VolunteerOpportunity { get; set; }
}

public class VolunteerAttendanceRecord : IAuditable
{
    public int Id { get; set; }
    public int VolunteerOpportunityId { get; set; }
    public int VolunteerUserId { get; set; }
    public DateTime AttendanceDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public decimal Hours { get; set; }
    public VolunteerAttendanceStatus Status { get; set; } = VolunteerAttendanceStatus.Present;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public VolunteerOpportunity? VolunteerOpportunity { get; set; }
    public VolunteerUser? VolunteerUser { get; set; }
}
