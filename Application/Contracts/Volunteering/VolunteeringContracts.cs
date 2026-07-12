using Domain.Entities;

namespace Application.Contracts.Volunteering;

public record VolunteeringDashboardResponse(int UsersCount, int ActiveUsersCount, int SubmittedRequestsCount, int ApprovedRequestsCount, int OpenOpportunitiesCount, int CompletedTasksCount, decimal AttendanceHours);

public record VolunteerUserResponse(int Id, string VolunteerNumber, string FullName, string? NationalId, string Mobile, string? Email, string? Skills, string Status, DateTime JoinedAt, string? Notes);
public record SaveVolunteerUserRequest(string VolunteerNumber, string FullName, string? NationalId, string Mobile, string? Email, string? Skills, VolunteerUserStatus Status, DateTime JoinedAt, string? Notes);

public record VolunteerRequestResponse(int Id, string RequestNumber, string Source, string ApplicantName, string Mobile, string? OpportunityTitle, string Status, DateTime RequestDate, string? DecisionNote, string? Notes, int? VolunteerUserId, string? VolunteerName, int? VolunteerOpportunityId, string? LinkedOpportunityTitle);
public record SaveVolunteerRequestRequest(string RequestNumber, VolunteerRequestSource Source, string ApplicantName, string Mobile, string? OpportunityTitle, VolunteerRequestStatus Status, DateTime RequestDate, string? DecisionNote, string? Notes, int? VolunteerUserId, int? VolunteerOpportunityId);
public record UpdateVolunteerRequestStatusRequest(VolunteerRequestStatus Status, string? DecisionNote);
public record ConvertVolunteerRequestRequest(string? VolunteerNumber, int? VolunteerOpportunityId, string? Skills, DateTime? JoinedAt, string? Notes, string? DecisionNote);
public record VolunteerRequestConversionResponse(VolunteerRequestResponse Request, VolunteerUserResponse VolunteerUser, VolunteerOpportunityResponse? Opportunity);

public record VolunteerOpportunityResponse(int Id, string OpportunityNumber, string Title, string? Description, string? Department, DateTime StartDate, DateTime? EndDate, int Seats, string Status, string? ProcedureNotes, string? ReportSummary, int RequestsCount, int TasksCount, int AttendanceCount);
public record SaveVolunteerOpportunityRequest(string OpportunityNumber, string Title, string? Description, string? Department, DateTime StartDate, DateTime? EndDate, int Seats, VolunteerOpportunityStatus Status, string? ProcedureNotes, string? ReportSummary);
public record SaveVolunteerOpportunityReportRequest(string? ProcedureNotes, string? ReportSummary, VolunteerOpportunityStatus? Status);

public record VolunteerOpportunityTaskResponse(int Id, int VolunteerOpportunityId, string OpportunityTitle, string Title, string? AssignedTo, DateTime? DueDate, string Status, string? Notes);
public record SaveVolunteerOpportunityTaskRequest(int VolunteerOpportunityId, string Title, string? AssignedTo, DateTime? DueDate, VolunteerTaskStatus Status, string? Notes);

public record VolunteerAttendanceResponse(int Id, int VolunteerOpportunityId, string OpportunityTitle, int VolunteerUserId, string VolunteerName, DateTime AttendanceDate, decimal Hours, string Status, string? Notes);
public record SaveVolunteerAttendanceRequest(int VolunteerOpportunityId, int VolunteerUserId, DateTime AttendanceDate, decimal Hours, VolunteerAttendanceStatus Status, string? Notes);
