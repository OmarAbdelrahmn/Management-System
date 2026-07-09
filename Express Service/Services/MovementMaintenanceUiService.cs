using Application.Contracts.MovementMaintenance;
using Application.Service.MovementMaintenance;
using Domain.Entities;

namespace Express_Service.Services;

public class MovementMaintenanceUiService(IMovementMaintenanceService service)
{
    public async Task<MovementMaintenanceDashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<List<FleetVehicleResponse>> GetVehiclesAsync(FleetVehicleStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetVehiclesAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveVehicleAsync(int? id, SaveFleetVehicleRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveVehicleAsync(id, request, cancellationToken), "تم حفظ السيارة.");
    public async Task<List<VehicleRequestResponse>> GetVehicleRequestsAsync(VehicleRequestStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetVehicleRequestsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveVehicleRequestAsync(int? id, SaveVehicleRequestRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveVehicleRequestAsync(id, request, cancellationToken), "تم حفظ طلب السيارة.");
    public async Task<(bool Success, string Message)> UpdateVehicleRequestStatusAsync(int id, VehicleRequestStatus status, string? note = null, CancellationToken cancellationToken = default) => ToUi(await service.UpdateVehicleRequestStatusAsync(id, new UpdateVehicleRequestStatusRequest(status, note), cancellationToken), "تم تحديث طلب السيارة.");
    public async Task<List<VehicleAssignmentResponse>> GetVehicleAssignmentsAsync(VehicleAssignmentStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetVehicleAssignmentsAsync(status, cancellationToken));
    public async Task<(bool Success, string Message)> HandVehicleAsync(SaveVehicleAssignmentRequest request, CancellationToken cancellationToken = default) => ToUi(await service.HandVehicleAsync(request, cancellationToken), "تم تسليم السيارة.");
    public async Task<(bool Success, string Message)> ReceiveVehicleAsync(int assignmentId, ReceiveVehicleRequest request, CancellationToken cancellationToken = default) => ToUi(await service.ReceiveVehicleAsync(assignmentId, request, cancellationToken), "تم استلام السيارة.");
    public async Task<List<MaintenanceRequestResponse>> GetMaintenanceRequestsAsync(MaintenanceRequestType? type = null, MaintenanceRequestStatus? status = null, CancellationToken cancellationToken = default) => ToList(await service.GetMaintenanceRequestsAsync(type, status, cancellationToken));
    public async Task<(bool Success, string Message)> SaveMaintenanceRequestAsync(int? id, SaveMaintenanceRequestRequest request, CancellationToken cancellationToken = default) => ToUi(await service.SaveMaintenanceRequestAsync(id, request, cancellationToken), "تم حفظ طلب الصيانة.");
    public async Task<(bool Success, string Message)> UpdateMaintenanceStatusAsync(int id, MaintenanceRequestStatus status, decimal? actualCost = null, string? note = null, CancellationToken cancellationToken = default) => ToUi(await service.UpdateMaintenanceStatusAsync(id, new UpdateMaintenanceStatusRequest(status, actualCost, note), cancellationToken), "تم تحديث طلب الصيانة.");

    private static (bool Success, string Message) ToUi<T>(Application.Abstraction.Result<T> result, string successMessage) =>
        result.IsSuccess ? (true, successMessage) : (false, result.Error.Description);

    private static List<T> ToList<T>(Application.Abstraction.Result<IEnumerable<T>> result) =>
        result.IsSuccess ? result.Value.ToList() : [];
}
