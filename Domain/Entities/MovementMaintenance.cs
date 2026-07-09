using Domain.Auditing;

namespace Domain.Entities;

public enum FleetVehicleStatus
{
    Available = 0,
    Assigned = 1,
    InMaintenance = 2,
    Retired = 3
}

public enum VehicleRequestStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Fulfilled = 3,
    Cancelled = 4
}

public enum VehicleAssignmentStatus
{
    Handed = 0,
    Received = 1,
    Overdue = 2
}

public enum MaintenanceRequestType
{
    Vehicle = 0,
    Building = 1,
    Equipment = 2,
    Other = 3
}

public enum MaintenanceRequestStatus
{
    Requested = 0,
    Approved = 1,
    InProgress = 2,
    Completed = 3,
    Rejected = 4,
    Cancelled = 5
}

public class FleetVehicle : IAuditable
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string? Odometer { get; set; }
    public FleetVehicleStatus Status { get; set; } = FleetVehicleStatus.Available;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class VehicleRequest : IAuditable
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public DateTime RequestedFrom { get; set; }
    public DateTime RequestedTo { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public VehicleRequestStatus Status { get; set; } = VehicleRequestStatus.Requested;
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class VehicleAssignment : IAuditable
{
    public int Id { get; set; }
    public int FleetVehicleId { get; set; }
    public int? VehicleRequestId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime HandedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public DateTime? ExpectedReturnAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? HandOdometer { get; set; }
    public string? ReceiveOdometer { get; set; }
    public VehicleAssignmentStatus Status { get; set; } = VehicleAssignmentStatus.Handed;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FleetVehicle? FleetVehicle { get; set; }
    public VehicleRequest? VehicleRequest { get; set; }
}

public class MaintenanceRequest : IAuditable
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public MaintenanceRequestType RequestType { get; set; } = MaintenanceRequestType.Other;
    public int? FleetVehicleId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow.AddHours(3);
    public MaintenanceRequestStatus Status { get; set; } = MaintenanceRequestStatus.Requested;
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public string? VendorName { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public FleetVehicle? FleetVehicle { get; set; }
}
