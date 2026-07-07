using Domain.Entities;

namespace Application.Contracts.Meetings;

public record CreateMeetingRequest(
    int BoardCycleId,
    string Title,
    DateTime ScheduledAt,
    int? AcceptanceDeadlineDays,
    int Serial = 0,
    MeetingType Type = MeetingType.General,
    string? Category = null,
    MeetingImportance Importance = MeetingImportance.Normal,
    bool HasVoting = true,
    bool IsOnline = false,
    string? Platform = null,
    string? Location = null,
    bool ReminderEnabled = false,
    DateTime? ReminderAt = null,
    int? DurationMinutes = null,
    MeetingRepeatMode RepeatMode = MeetingRepeatMode.None,
    DateTime? RepeatUntil = null,
    decimal MinimumAttendancePercentage = 100,
    int? AvailableSeats = null,
    string? Notes = null,
    IEnumerable<string>? ManagerUserIds = null,
    IEnumerable<string>? CandidateUserIds = null,
    IEnumerable<CreateMeetingGuestRequest>? Guests = null);

public record CreateMeetingGuestRequest(
    string FullName,
    string? Email,
    string? PhoneNumber);

public record AddAgendaItemRequest(
    string Title,
    string Description,
    bool RequiresDecision);

public record MeetingResponse(
    int Id,
    int BoardCycleId,
    string Title,
    DateTime ScheduledAt,
    int AcceptanceDeadlineDays,
    int Serial,
    string Type,
    string? Category,
    string Importance,
    bool HasVoting,
    bool IsOnline,
    string? Platform,
    string? Location,
    bool ReminderEnabled,
    DateTime? ReminderAt,
    int? DurationMinutes,
    string RepeatMode,
    DateTime? RepeatUntil,
    decimal MinimumAttendancePercentage,
    int? AvailableSeats,
    string? Notes,
    string Status,
    IEnumerable<string> ManagerUserIds,
    IEnumerable<string> CandidateUserIds,
    IEnumerable<MeetingGuestResponse> Guests,
    IEnumerable<MeetingRepeatDraftResponse> RepeatDrafts,
    IEnumerable<AgendaItemResponse> AgendaItems);

public record MeetingGuestResponse(
    int Id,
    string FullName,
    string? Email,
    string? PhoneNumber);

public record MeetingListItemResponse(
    int Id,
    string Title,
    DateTime ScheduledAt,
    string Type,
    string Status,
    bool IsOnline,
    string? Location,
    string? Platform);

public record MeetingCalendarItemResponse(
    int Id,
    string Title,
    DateTime StartsAt,
    DateTime? EndsAt,
    string Type,
    string Status);

public record MeetingRepeatDraftResponse(
    int Id,
    int SourceBoardMeetingId,
    int? CreatedBoardMeetingId,
    DateTime ScheduledAt);

public record SubmitMeetingApprovalRequest(string? Comments);

public record DecideMeetingApprovalRequest(string ApproverUserId, string? Comments);

public record AgendaItemResponse(
    int Id,
    int ItemNumber,
    string Title,
    string Description,
    bool RequiresDecision,
    string Status,
    string? RejectionText);

public record MeetingNoteRequest(
    string Text,
    MeetingNoteVisibility Visibility,
    int? MeetingAgendaItemId);
