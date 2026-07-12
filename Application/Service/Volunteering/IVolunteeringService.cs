using Application.Abstraction;
using Application.Contracts.Volunteering;
using Domain.Entities;

namespace Application.Service.Volunteering;

public interface IVolunteeringService
{
    Task<Result<VolunteeringDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VolunteerUserResponse>>> GetUsersAsync(VolunteerUserStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VolunteerUserResponse>> SaveUserAsync(int? id, SaveVolunteerUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VolunteerRequestResponse>>> GetRequestsAsync(VolunteerRequestSource? source = null, VolunteerRequestStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VolunteerRequestResponse>> SaveRequestAsync(int? id, SaveVolunteerRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateRequestStatusAsync(int id, UpdateVolunteerRequestStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<VolunteerRequestConversionResponse>> ConvertRequestToVolunteerAsync(int id, ConvertVolunteerRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VolunteerOpportunityResponse>>> GetOpportunitiesAsync(VolunteerOpportunityStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VolunteerOpportunityResponse>> SaveOpportunityAsync(int? id, SaveVolunteerOpportunityRequest request, CancellationToken cancellationToken = default);
    Task<Result> SaveOpportunityReportAsync(int id, SaveVolunteerOpportunityReportRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VolunteerOpportunityTaskResponse>>> GetTasksAsync(int? opportunityId = null, VolunteerTaskStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VolunteerOpportunityTaskResponse>> SaveTaskAsync(int? id, SaveVolunteerOpportunityTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VolunteerAttendanceResponse>>> GetAttendanceAsync(int? opportunityId = null, int? volunteerUserId = null, CancellationToken cancellationToken = default);
    Task<Result<VolunteerAttendanceResponse>> SaveAttendanceAsync(int? id, SaveVolunteerAttendanceRequest request, CancellationToken cancellationToken = default);
}
