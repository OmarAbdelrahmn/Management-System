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
        var savedVehicle = Assert.Single(vehicles.Value);
        Assert.Equal("Available", savedVehicle.Status);
        Assert.Equal("1200", savedVehicle.Odometer);

        var fulfilledRequests = await service.GetVehicleRequestsAsync(VehicleRequestStatus.Fulfilled);
        Assert.Equal(request.Value.Id, Assert.Single(fulfilledRequests.Value).Id);
    }

    [Fact]
    public async Task FleetWorkflow_RequiresApprovedRequestAndAvailableVehicle()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MovementMaintenanceService(dbcontext);

        var vehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("DEF-789", "Hyundai", 2025, null, null, FleetVehicleStatus.Available, null));
        var request = await service.SaveVehicleRequestAsync(null, new SaveVehicleRequestRequest("موظف", new DateTime(2026, 7, 8), new DateTime(2026, 7, 9), "زيارة", VehicleRequestStatus.Requested, null));

        var beforeApproval = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(vehicle.Value.Id, request.Value.Id, "موظف", new DateTime(2026, 7, 8, 8, 0, 0), new DateTime(2026, 7, 9), "1000", null));
        await service.UpdateVehicleRequestStatusAsync(request.Value.Id, new UpdateVehicleRequestStatusRequest(VehicleRequestStatus.Approved, "معتمد"));
        var handed = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(vehicle.Value.Id, request.Value.Id, "موظف", new DateTime(2026, 7, 8, 8, 0, 0), new DateTime(2026, 7, 9), "1000", null));
        var secondRequest = await service.SaveVehicleRequestAsync(null, new SaveVehicleRequestRequest("موظف آخر", new DateTime(2026, 7, 8), new DateTime(2026, 7, 9), "زيارة أخرى", VehicleRequestStatus.Approved, null));
        var alreadyAssigned = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(vehicle.Value.Id, secondRequest.Value.Id, "موظف آخر", new DateTime(2026, 7, 8, 9, 0, 0), new DateTime(2026, 7, 9), "1000", null));

        Assert.False(beforeApproval.IsSuccess);
        Assert.True(handed.IsSuccess);
        Assert.False(alreadyAssigned.IsSuccess);
    }

    [Fact]
    public async Task FleetWorkflow_MarksOnlyExpiredAssignmentsAsOverdue()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MovementMaintenanceService(dbcontext);

        var lateVehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("LATE-1", "Toyota", 2025, null, "1000", FleetVehicleStatus.Available, null));
        var activeVehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("OK-1", "Kia", 2025, null, "2000", FleetVehicleStatus.Available, null));
        var lateRequest = await service.SaveVehicleRequestAsync(null, new SaveVehicleRequestRequest("موظف متأخر", new DateTime(2026, 7, 8), new DateTime(2026, 7, 9), "زيارة", VehicleRequestStatus.Approved, null));
        var activeRequest = await service.SaveVehicleRequestAsync(null, new SaveVehicleRequestRequest("موظف ملتزم", new DateTime(2026, 7, 8), new DateTime(2026, 7, 12), "زيارة", VehicleRequestStatus.Approved, null));
        var lateAssignment = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(lateVehicle.Value.Id, lateRequest.Value.Id, "موظف متأخر", new DateTime(2026, 7, 8, 8, 0, 0), new DateTime(2026, 7, 9, 17, 0, 0), "1000", null));
        var activeAssignment = await service.HandVehicleAsync(new SaveVehicleAssignmentRequest(activeVehicle.Value.Id, activeRequest.Value.Id, "موظف ملتزم", new DateTime(2026, 7, 8, 8, 0, 0), new DateTime(2026, 7, 12, 17, 0, 0), "2000", null));

        var overdue = await service.MarkOverdueAssignmentsAsync(new MarkOverdueVehicleAssignmentsRequest(new DateTime(2026, 7, 10, 9, 0, 0), "تأخر عن موعد الإرجاع"));
        var overdueAssignments = await service.GetVehicleAssignmentsAsync(VehicleAssignmentStatus.Overdue);
        var handedAssignments = await service.GetVehicleAssignmentsAsync(VehicleAssignmentStatus.Handed);
        var dashboard = await service.GetDashboardAsync();

        Assert.True(lateAssignment.IsSuccess);
        Assert.True(activeAssignment.IsSuccess);
        Assert.True(overdue.IsSuccess);
        Assert.Equal(1, overdue.Value.UpdatedCount);
        Assert.Equal(lateAssignment.Value.Id, Assert.Single(overdueAssignments.Value).Id);
        Assert.Equal(activeAssignment.Value.Id, Assert.Single(handedAssignments.Value).Id);
        Assert.Equal(2, dashboard.Value.ActiveAssignmentsCount);
        Assert.Contains("تأخر", overdue.Value.Assignments.Single().Notes);
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
        var vehicles = await service.GetVehiclesAsync();

        Assert.True(request.IsSuccess);
        Assert.Equal("Completed", completed.Value.Status);
        Assert.Equal("Available", Assert.Single(vehicles.Value).Status);
        Assert.Equal(900, dashboard.Value.MaintenanceActualCost);
    }

    [Fact]
    public async Task MaintenanceWorkflow_UpdatesVehicleStateOnlyForActiveWork()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var service = new MovementMaintenanceService(dbcontext);
        var vehicle = await service.SaveVehicleAsync(null, new SaveFleetVehicleRequest("MNO-321", "Ford", 2023, null, null, FleetVehicleStatus.Available, null));

        var request = await service.SaveMaintenanceRequestAsync(null, new SaveMaintenanceRequestRequest(
            MaintenanceRequestType.Vehicle,
            vehicle.Value.Id,
            "مسؤول الحركة",
            "MNO-321",
            "فحص دوري",
            new DateTime(2026, 7, 8),
            MaintenanceRequestStatus.Requested,
            500,
            0,
            null,
            null));
        var requestedVehicles = await service.GetVehiclesAsync();
        var approved = await service.UpdateMaintenanceStatusAsync(request.Value.Id, new UpdateMaintenanceStatusRequest(MaintenanceRequestStatus.Approved, null, "معتمدة"));
        var activeVehicles = await service.GetVehiclesAsync();
        var cancelled = await service.UpdateMaintenanceStatusAsync(request.Value.Id, new UpdateMaintenanceStatusRequest(MaintenanceRequestStatus.Cancelled, null, "ألغيت"));
        var cancelledVehicles = await service.GetVehiclesAsync();

        Assert.True(request.IsSuccess);
        Assert.Equal("Available", Assert.Single(requestedVehicles.Value).Status);
        Assert.True(approved.IsSuccess);
        Assert.Equal("InMaintenance", Assert.Single(activeVehicles.Value).Status);
        Assert.True(cancelled.IsSuccess);
        Assert.Equal("Available", Assert.Single(cancelledVehicles.Value).Status);
    }
}
