using Application.Abstraction;
using Application.Contracts.MovementMaintenance;
using Domain.Entities;

namespace Application.Service.MovementMaintenance;

public interface IMovementMaintenanceService
{
    Task<Result<MovementMaintenanceDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FleetVehicleResponse>>> GetVehiclesAsync(FleetVehicleStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<FleetVehicleResponse>> SaveVehicleAsync(int? id, SaveFleetVehicleRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VehicleRequestResponse>>> GetVehicleRequestsAsync(VehicleRequestStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VehicleRequestResponse>> SaveVehicleRequestAsync(int? id, SaveVehicleRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<VehicleRequestResponse>> UpdateVehicleRequestStatusAsync(int id, UpdateVehicleRequestStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VehicleAssignmentResponse>>> GetVehicleAssignmentsAsync(VehicleAssignmentStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<VehicleAssignmentResponse>> HandVehicleAsync(SaveVehicleAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<VehicleAssignmentResponse>> ReceiveVehicleAsync(int assignmentId, ReceiveVehicleRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MaintenanceRequestResponse>>> GetMaintenanceRequestsAsync(MaintenanceRequestType? type = null, MaintenanceRequestStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<MaintenanceRequestResponse>> SaveMaintenanceRequestAsync(int? id, SaveMaintenanceRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<MaintenanceRequestResponse>> UpdateMaintenanceStatusAsync(int id, UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default);
}
