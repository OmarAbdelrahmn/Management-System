using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class MovementMaintenanceErrors
{
    public static readonly Error VehicleNotFound = new("MovementMaintenance.VehicleNotFound", "Fleet vehicle was not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicatePlate = new("MovementMaintenance.DuplicatePlate", "Vehicle plate number is already used.", StatusCodes.Status409Conflict);
    public static readonly Error VehicleRequestNotFound = new("MovementMaintenance.VehicleRequestNotFound", "Vehicle request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AssignmentNotFound = new("MovementMaintenance.AssignmentNotFound", "Vehicle assignment was not found.", StatusCodes.Status404NotFound);
    public static readonly Error MaintenanceRequestNotFound = new("MovementMaintenance.MaintenanceRequestNotFound", "Maintenance request was not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidRequest = new("MovementMaintenance.InvalidRequest", "Movement and maintenance request is invalid.", StatusCodes.Status400BadRequest);
}
