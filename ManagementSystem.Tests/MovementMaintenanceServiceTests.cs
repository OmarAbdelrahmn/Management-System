using Application.Contracts.MovementMaintenance;
using Application.Service.MovementMaintenance;
using Domain.Entities;

namespace ManagementSystem.Tests;

public class MovementMaintenanceServiceTests
{
    [Fact]
    public async Task FleetWorkflow_RequestHandReceiveVehicle()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MovementMaintenanceService(dbcontext);

        var vehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("ABC-123", "Toyota", 2025, "White", "1000", FleetVehicleStatus.Available, null));
        var request = await service.SaveVehicleRequestAsync(null, new SaveVehicleRequestRequest("موظف", new DateTime(2026, 7, 8), new DateTime(2026, 7, 9), "زيارة ميدانية", VehicleRequestStatus.Approved, null));
        var handed = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(vehicle.Value.Id, request.Value.Id, "موظف", new DateTime(2026, 7, 8, 8, 0, 0), new DateTime(2026, 7, 9), "1000", null));
        var received = await service.ReceiveVehicleAsync(handed.Value.Id, new ReceiveVehicleRequest(new DateTime(2026, 7, 9, 16, 0, 0), "1200", "سليم"));
        var vehicles = await service.GetVehiclesAsync();

        Assert.True(handed.IsSuccess);
        Assert.Equal("Handed", handed.Value.Status);
        Assert.Equal("Received", received.Value.Status);
        Assert.Equal("Available", Assert.Single(vehicles.Value).Status);
    }

    [Fact]
    public async Task MaintenanceWorkflow_SaveAndCompleteVehicleRequest()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MovementMaintenanceService(dbcontext);
        var vehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("XYZ-456", "Nissan", 2024, null, null, FleetVehicleStatus.Available, null));

        var request = await service.SaveMaintenanceRequestAsync(null, new SaveMaintenanceRequestRequest(
            MaintenanceRequestType.Vehicle,
            vehicle.Value.Id,
            "مسؤول الحركة",
            "XYZ-456",
            "تغيير الإطارات",
            new DateTime(2026, 7, 8),
            MaintenanceRequestStatus.InProgress,
            1000,
            0,
            "مركز صيانة",
            null));
        var completed = await service.UpdateMaintenanceStatusAsync(request.Value.Id, new UpdateMaintenanceStatusRequest(MaintenanceRequestStatus.Completed, 900, "تمت الصيانة"));
        var dashboard = await service.GetDashboardAsync();

        Assert.True(request.IsSuccess);
        Assert.Equal("Completed", completed.Value.Status);
        Assert.Equal(900, dashboard.Value.MaintenanceActualCost);
    }
}
