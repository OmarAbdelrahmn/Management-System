using Application.Contracts.Volunteering;
using Application.Service.Volunteering;
using Domain.Entities;

namespace Express_Service.Services;

public class VolunteeringUiService(IVolunteeringService service)
{
    public async Task<VolunteeringDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<VolunteerUserResponse>> GetUsersAsync(VolunteerUserStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetUsersAsync(status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveUserAsync(int? id, SaveVolunteerUserRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveUserAsync(id, request, cancellationToken), "تم حفظ حساب المتطوع.");

    public async Task<List<VolunteerRequestResponse>> GetRequestsAsync(VolunteerRequestSource? source = null, VolunteerRequestStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetRequestsAsync(source, status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveRequestAsync(int? id, SaveVolunteerRequestRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveRequestAsync(id, request, cancellationToken), "تم حفظ طلب التطوع.");

    public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int id, VolunteerRequestStatus status, string? note = null, CancellationToken cancellationToken = default) =>
        ToUi(await service.UpdateRequestStatusAsync(id, new UpdateVolunteerRequestStatusRequest(status, note), cancellationToken), "تم تحديث طلب التطوع.");

    public async Task<(bool Success, string Message)> ConvertRequestToVolunteerAsync(int id, ConvertVolunteerRequestRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.ConvertRequestToVolunteerAsync(id, request, cancellationToken), "تم تحويل الطلب إلى حساب متطوع وربطه بالفرصة.");

    public async Task<List<VolunteerOpportunityResponse>> GetOpportunitiesAsync(VolunteerOpportunityStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetOpportunitiesAsync(status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveOpportunityAsync(int? id, SaveVolunteerOpportunityRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveOpportunityAsync(id, request, cancellationToken), "تم حفظ الفرصة التطوعية.");

    public async Task<(bool Success, string Message)> SaveOpportunityReportAsync(int id, SaveVolunteerOpportunityReportRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveOpportunityReportAsync(id, request, cancellationToken), "تم حفظ تقرير الفرصة.");

    public async Task<List<VolunteerOpportunityTaskResponse>> GetTasksAsync(int? opportunityId = null, VolunteerTaskStatus? status = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetTasksAsync(opportunityId, status, cancellationToken));

    public async Task<(bool Success, string Message)> SaveTaskAsync(int? id, SaveVolunteerOpportunityTaskRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveTaskAsync(id, request, cancellationToken), "تم حفظ مهمة الفرصة.");

    public async Task<List<VolunteerAttendanceResponse>> GetAttendanceAsync(int? opportunityId = null, int? volunteerUserId = null, CancellationToken cancellationToken = default) =>
        ToList(await service.GetAttendanceAsync(opportunityId, volunteerUserId, cancellationToken));

    public async Task<(bool Success, string Message)> SaveAttendanceAsync(int? id, SaveVolunteerAttendanceRequest request, CancellationToken cancellationToken = default) =>
        ToUi(await service.SaveAttendanceAsync(id, request, cancellationToken), "تم حفظ سجل الحضور.");

    private static (bool Success, string Message) ToUi(Application.Abstraction.Result result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
