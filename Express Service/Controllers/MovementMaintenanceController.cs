using Application.Contracts.MovementMaintenance;
using Application.Service.MovementMaintenance;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Secretary,Chairman")]
public class MovementMaintenanceController(IMovementMaintenanceService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => ToAction(await service.GetDashboardAsync(ct));
    [HttpGet("vehicles")]
    public async Task<IActionResult> Vehicles([FromQuery] FleetVehicleStatus? status, CancellationToken ct) => ToAction(await service.GetVehiclesAsync(status, ct));
    [HttpPost("vehicles")]
    public async Task<IActionResult> SaveVehicle([FromBody] SaveFleetVehicleRequest request, CancellationToken ct) => ToAction(await service.SaveVehicleAsync(null, request, ct));
    [HttpGet("vehicle-requests")]
    public async Task<IActionResult> VehicleRequests([FromQuery] VehicleRequestStatus? status, CancellationToken ct) => ToAction(await service.GetVehicleRequestsAsync(status, ct));
    [HttpPost("vehicle-requests")]
    public async Task<IActionResult> SaveVehicleRequest([FromBody] SaveVehicleRequestRequest request, CancellationToken ct) => ToAction(await service.SaveVehicleRequestAsync(null, request, ct));
    [HttpPost("vehicle-requests/{id:int}/status")]
    public async Task<IActionResult> UpdateVehicleRequest(int id, [FromBody] UpdateVehicleRequestStatusRequest request, CancellationToken ct) => ToAction(await service.UpdateVehicleRequestStatusAsync(id, request, ct));
    [HttpGet("assignments")]
    public async Task<IActionResult> Assignments([FromQuery] VehicleAssignmentStatus? status, CancellationToken ct) => ToAction(await service.GetVehicleAssignmentsAsync(status, ct));
    [HttpPost("assignments")]
    public async Task<IActionResult> HandVehicle([FromBody] SaveVehicleAssignmentRequest request, CancellationToken ct) => ToAction(await service.HandVehicleAsync(request, ct));
    [HttpPost("assignments/{id:int}/receive")]
    public async Task<IActionResult> ReceiveVehicle(int id, [FromBody] ReceiveVehicleRequest request, CancellationToken ct) => ToAction(await service.ReceiveVehicleAsync(id, request, ct));
    [HttpGet("maintenance")]
    public async Task<IActionResult> Maintenance([FromQuery] MaintenanceRequestType? type, [FromQuery] MaintenanceRequestStatus? status, CancellationToken ct) => ToAction(await service.GetMaintenanceRequestsAsync(type, status, ct));
    [HttpPost("maintenance")]
    public async Task<IActionResult> SaveMaintenance([FromBody] SaveMaintenanceRequestRequest request, CancellationToken ct) => ToAction(await service.SaveMaintenanceRequestAsync(null, request, ct));
    [HttpPost("maintenance/{id:int}/status")]
    public async Task<IActionResult> UpdateMaintenance(int id, [FromBody] UpdateMaintenanceStatusRequest request, CancellationToken ct) => ToAction(await service.UpdateMaintenanceStatusAsync(id, request, ct));

    private IActionResult ToAction<T>(Application.Abstraction.Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.ToProblem();
}
