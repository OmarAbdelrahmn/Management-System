using Domain.Entities;

namespace Application.Contracts.MovementMaintenance;

public record MovementMaintenanceDashboardResponse(int VehiclesCount, int AvailableVehiclesCount, int OpenVehicleRequestsCount, int ActiveAssignmentsCount, int OpenMaintenanceRequestsCount, decimal MaintenanceActualCost);

public record FleetVehicleResponse(int Id, string PlateNumber, string Model, int Year, string? Color, string? Odometer, string Status, string? Notes);
public record SaveFleetVehicleRequest(string PlateNumber, string Model, int Year, string? Color, string? Odometer, FleetVehicleStatus Status, string? Notes);

public record VehicleRequestResponse(int Id, string RequestNumber, string RequesterName, DateTime RequestedFrom, DateTime RequestedTo, string Purpose, string Status, string? DecisionNote);
public record SaveVehicleRequestRequest(string RequesterName, DateTime RequestedFrom, DateTime RequestedTo, string Purpose, VehicleRequestStatus Status, string? DecisionNote);
public record UpdateVehicleRequestStatusRequest(VehicleRequestStatus Status, string? DecisionNote);

public record VehicleAssignmentResponse(int Id, int FleetVehicleId, string VehiclePlateNumber, int? VehicleRequestId, string RequestNumber, string EmployeeName, DateTime HandedAt, DateTime? ExpectedReturnAt, DateTime? ReceivedAt, string? HandOdometer, string? ReceiveOdometer, string Status, string? Notes);
public record SaveVehicleAssignmentRequest(int FleetVehicleId, int? VehicleRequestId, string EmployeeName, DateTime HandedAt, DateTime? ExpectedReturnAt, string? HandOdometer, string? Notes);
public record ReceiveVehicleRequest(DateTime ReceivedAt, string? ReceiveOdometer, string? Notes);

public record MaintenanceRequestResponse(int Id, string RequestNumber, string RequestType, int? FleetVehicleId, string VehiclePlateNumber, string RequestedBy, string AssetName, string IssueDescription, DateTime RequestDate, string Status, decimal EstimatedCost, decimal ActualCost, string? VendorName, string? CompletionNotes);
public record SaveMaintenanceRequestRequest(MaintenanceRequestType RequestType, int? FleetVehicleId, string RequestedBy, string AssetName, string IssueDescription, DateTime RequestDate, MaintenanceRequestStatus Status, decimal EstimatedCost, decimal ActualCost, string? VendorName, string? CompletionNotes);
public record UpdateMaintenanceStatusRequest(MaintenanceRequestStatus Status, decimal? ActualCost, string? CompletionNotes);
