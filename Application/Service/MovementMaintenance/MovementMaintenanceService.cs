using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.MovementMaintenance;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Service.MovementMaintenance;

public class MovementMaintenanceService(ApplicationDbcontext dbcontext) : IMovementMaintenanceService
{
    public async Task<Result<MovementMaintenanceDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return Result.Success(new MovementMaintenanceDashboardResponse(
            await dbcontext.FleetVehicles.CountAsync(cancellationToken),
            await dbcontext.FleetVehicles.CountAsync(x => x.Status == FleetVehicleStatus.Available, cancellationToken),
            await dbcontext.VehicleRequests.CountAsync(x => x.Status == VehicleRequestStatus.Requested || x.Status == VehicleRequestStatus.Approved, cancellationToken),
            await dbcontext.VehicleAssignments.CountAsync(x => x.Status == VehicleAssignmentStatus.Handed || x.Status == VehicleAssignmentStatus.Overdue, cancellationToken),
            await dbcontext.MaintenanceRequests.CountAsync(x => x.Status == MaintenanceRequestStatus.Requested || x.Status == MaintenanceRequestStatus.Approved || x.Status == MaintenanceRequestStatus.InProgress, cancellationToken),
            await dbcontext.MaintenanceRequests.Where(x => x.Status == MaintenanceRequestStatus.Completed).SumAsync(x => x.ActualCost, cancellationToken)));
    }

    public async Task<Result<IEnumerable<FleetVehicleResponse>>> GetVehiclesAsync(FleetVehicleStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.FleetVehicles.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<FleetVehicleResponse>>(await query.OrderBy(x => x.PlateNumber).Select(x => MapVehicle(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<FleetVehicleResponse>> SaveVehicleAsync(int? id, SaveFleetVehicleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PlateNumber) || string.IsNullOrWhiteSpace(request.Model) || request.Year < 1980)
            return Result.Failure<FleetVehicleResponse>(MovementMaintenanceErrors.InvalidRequest);

        var plate = request.PlateNumber.Trim();
        if (await dbcontext.FleetVehicles.AnyAsync(x => x.PlateNumber == plate && (!id.HasValue || x.Id != id.Value), cancellationToken))
            return Result.Failure<FleetVehicleResponse>(MovementMaintenanceErrors.DuplicatePlate);

        var entity = id.HasValue
            ? await dbcontext.FleetVehicles.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new FleetVehicle();
        if (entity is null)
            return Result.Failure<FleetVehicleResponse>(MovementMaintenanceErrors.VehicleNotFound);
        if (!id.HasValue) dbcontext.FleetVehicles.Add(entity);

        entity.PlateNumber = plate;
        entity.Model = request.Model.Trim();
        entity.Year = request.Year;
        entity.Color = request.Color?.Trim();
        entity.Odometer = request.Odometer?.Trim();
        entity.Status = request.Status;
        entity.Notes = request.Notes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapVehicle(entity));
    }

    public async Task<Result<IEnumerable<VehicleRequestResponse>>> GetVehicleRequestsAsync(VehicleRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VehicleRequests.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<VehicleRequestResponse>>(await query.OrderByDescending(x => x.RequestedFrom).Select(x => MapRequest(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<VehicleRequestResponse>> SaveVehicleRequestAsync(int? id, SaveVehicleRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequesterName) || string.IsNullOrWhiteSpace(request.Purpose) || request.RequestedTo < request.RequestedFrom)
            return Result.Failure<VehicleRequestResponse>(MovementMaintenanceErrors.InvalidRequest);

        var entity = id.HasValue
            ? await dbcontext.VehicleRequests.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new VehicleRequest { RequestNumber = await GenerateRequestNumberAsync("CAR", cancellationToken) };
        if (entity is null)
            return Result.Failure<VehicleRequestResponse>(MovementMaintenanceErrors.VehicleRequestNotFound);
        if (!id.HasValue) dbcontext.VehicleRequests.Add(entity);

        entity.RequesterName = request.RequesterName.Trim();
        entity.RequestedFrom = request.RequestedFrom;
        entity.RequestedTo = request.RequestedTo;
        entity.Purpose = request.Purpose.Trim();
        entity.Status = request.Status;
        entity.DecisionNote = request.DecisionNote?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRequest(entity));
    }

    public async Task<Result<VehicleRequestResponse>> UpdateVehicleRequestStatusAsync(int id, UpdateVehicleRequestStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.VehicleRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<VehicleRequestResponse>(MovementMaintenanceErrors.VehicleRequestNotFound);

        entity.Status = request.Status;
        entity.DecisionNote = request.DecisionNote?.Trim();
        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapRequest(entity));
    }

    public async Task<Result<IEnumerable<VehicleAssignmentResponse>>> GetVehicleAssignmentsAsync(VehicleAssignmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.VehicleAssignments.AsNoTracking().Include(x => x.FleetVehicle).Include(x => x.VehicleRequest).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<VehicleAssignmentResponse>>(await query.OrderByDescending(x => x.HandedAt).Select(x => MapAssignment(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<VehicleAssignmentResponse>> HandVehicleAsync(SaveVehicleAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeName))
            return Result.Failure<VehicleAssignmentResponse>(MovementMaintenanceErrors.InvalidRequest);

        var vehicle = await dbcontext.FleetVehicles.FirstOrDefaultAsync(x => x.Id == request.FleetVehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<VehicleAssignmentResponse>(MovementMaintenanceErrors.VehicleNotFound);

        if (request.VehicleRequestId.HasValue && !await dbcontext.VehicleRequests.AnyAsync(x => x.Id == request.VehicleRequestId.Value, cancellationToken))
            return Result.Failure<VehicleAssignmentResponse>(MovementMaintenanceErrors.VehicleRequestNotFound);

        var entity = new VehicleAssignment
        {
            FleetVehicleId = vehicle.Id,
            VehicleRequestId = request.VehicleRequestId,
            EmployeeName = request.EmployeeName.Trim(),
            HandedAt = request.HandedAt,
            ExpectedReturnAt = request.ExpectedReturnAt,
            HandOdometer = request.HandOdometer?.Trim(),
            Status = VehicleAssignmentStatus.Handed,
            Notes = request.Notes?.Trim()
        };
        dbcontext.VehicleAssignments.Add(entity);
        vehicle.Status = FleetVehicleStatus.Assigned;

        if (request.VehicleRequestId.HasValue)
        {
            var carRequest = await dbcontext.VehicleRequests.FirstAsync(x => x.Id == request.VehicleRequestId.Value, cancellationToken);
            carRequest.Status = VehicleRequestStatus.Fulfilled;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.VehicleAssignments.AsNoTracking().Include(x => x.FleetVehicle).Include(x => x.VehicleRequest).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapAssignment(saved));
    }

    public async Task<Result<VehicleAssignmentResponse>> ReceiveVehicleAsync(int assignmentId, ReceiveVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.VehicleAssignments.Include(x => x.FleetVehicle).Include(x => x.VehicleRequest).FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
        if (entity is null)
            return Result.Failure<VehicleAssignmentResponse>(MovementMaintenanceErrors.AssignmentNotFound);

        entity.ReceivedAt = request.ReceivedAt;
        entity.ReceiveOdometer = request.ReceiveOdometer?.Trim();
        entity.Notes = request.Notes?.Trim() ?? entity.Notes;
        entity.Status = VehicleAssignmentStatus.Received;
        if (entity.FleetVehicle is not null)
        {
            entity.FleetVehicle.Status = FleetVehicleStatus.Available;
            entity.FleetVehicle.Odometer = entity.ReceiveOdometer ?? entity.FleetVehicle.Odometer;
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapAssignment(entity));
    }

    public async Task<Result<IEnumerable<MaintenanceRequestResponse>>> GetMaintenanceRequestsAsync(MaintenanceRequestType? type = null, MaintenanceRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbcontext.MaintenanceRequests.AsNoTracking().Include(x => x.FleetVehicle).AsQueryable();
        if (type.HasValue) query = query.Where(x => x.RequestType == type.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Result.Success<IEnumerable<MaintenanceRequestResponse>>(await query.OrderByDescending(x => x.RequestDate).Select(x => MapMaintenance(x)).ToListAsync(cancellationToken));
    }

    public async Task<Result<MaintenanceRequestResponse>> SaveMaintenanceRequestAsync(int? id, SaveMaintenanceRequestRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestedBy) || string.IsNullOrWhiteSpace(request.AssetName) || string.IsNullOrWhiteSpace(request.IssueDescription) || request.EstimatedCost < 0 || request.ActualCost < 0)
            return Result.Failure<MaintenanceRequestResponse>(MovementMaintenanceErrors.InvalidRequest);

        if (request.FleetVehicleId.HasValue && !await dbcontext.FleetVehicles.AnyAsync(x => x.Id == request.FleetVehicleId.Value, cancellationToken))
            return Result.Failure<MaintenanceRequestResponse>(MovementMaintenanceErrors.VehicleNotFound);

        var entity = id.HasValue
            ? await dbcontext.MaintenanceRequests.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken)
            : new MaintenanceRequest { RequestNumber = await GenerateRequestNumberAsync("MNT", cancellationToken) };
        if (entity is null)
            return Result.Failure<MaintenanceRequestResponse>(MovementMaintenanceErrors.MaintenanceRequestNotFound);
        if (!id.HasValue) dbcontext.MaintenanceRequests.Add(entity);

        entity.RequestType = request.RequestType;
        entity.FleetVehicleId = request.FleetVehicleId;
        entity.RequestedBy = request.RequestedBy.Trim();
        entity.AssetName = request.AssetName.Trim();
        entity.IssueDescription = request.IssueDescription.Trim();
        entity.RequestDate = request.RequestDate;
        entity.Status = request.Status;
        entity.EstimatedCost = request.EstimatedCost;
        entity.ActualCost = request.ActualCost;
        entity.VendorName = request.VendorName?.Trim();
        entity.CompletionNotes = request.CompletionNotes?.Trim();

        await dbcontext.SaveChangesAsync(cancellationToken);
        var saved = await dbcontext.MaintenanceRequests.AsNoTracking().Include(x => x.FleetVehicle).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return Result.Success(MapMaintenance(saved));
    }

    public async Task<Result<MaintenanceRequestResponse>> UpdateMaintenanceStatusAsync(int id, UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbcontext.MaintenanceRequests.Include(x => x.FleetVehicle).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Result.Failure<MaintenanceRequestResponse>(MovementMaintenanceErrors.MaintenanceRequestNotFound);

        entity.Status = request.Status;
        if (request.ActualCost.HasValue) entity.ActualCost = request.ActualCost.Value;
        entity.CompletionNotes = request.CompletionNotes?.Trim() ?? entity.CompletionNotes;
        if (entity.FleetVehicle is not null)
            entity.FleetVehicle.Status = request.Status == MaintenanceRequestStatus.Completed ? FleetVehicleStatus.Available : FleetVehicleStatus.InMaintenance;

        await dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(MapMaintenance(entity));
    }

    private async Task<string> GenerateRequestNumberAsync(string prefix, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.AddHours(3).Year;
        var token = $"{prefix}-{year}-";
        var vehicleCount = prefix == "CAR"
            ? await dbcontext.VehicleRequests.CountAsync(x => x.RequestNumber.StartsWith(token), cancellationToken)
            : await dbcontext.MaintenanceRequests.CountAsync(x => x.RequestNumber.StartsWith(token), cancellationToken);
        return $"{token}{vehicleCount + 1:0000}";
    }

    private static FleetVehicleResponse MapVehicle(FleetVehicle x) => new(x.Id, x.PlateNumber, x.Model, x.Year, x.Color, x.Odometer, x.Status.ToString(), x.Notes);
    private static VehicleRequestResponse MapRequest(VehicleRequest x) => new(x.Id, x.RequestNumber, x.RequesterName, x.RequestedFrom, x.RequestedTo, x.Purpose, x.Status.ToString(), x.DecisionNote);
    private static VehicleAssignmentResponse MapAssignment(VehicleAssignment x) => new(x.Id, x.FleetVehicleId, x.FleetVehicle?.PlateNumber ?? string.Empty, x.VehicleRequestId, x.VehicleRequest?.RequestNumber ?? string.Empty, x.EmployeeName, x.HandedAt, x.ExpectedReturnAt, x.ReceivedAt, x.HandOdometer, x.ReceiveOdometer, x.Status.ToString(), x.Notes);
    private static MaintenanceRequestResponse MapMaintenance(MaintenanceRequest x) => new(x.Id, x.RequestNumber, x.RequestType.ToString(), x.FleetVehicleId, x.FleetVehicle?.PlateNumber ?? string.Empty, x.RequestedBy, x.AssetName, x.IssueDescription, x.RequestDate, x.Status.ToString(), x.EstimatedCost, x.ActualCost, x.VendorName, x.CompletionNotes);
}
