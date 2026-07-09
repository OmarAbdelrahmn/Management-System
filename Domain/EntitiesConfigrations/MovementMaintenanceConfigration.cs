using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class FleetVehicleConfigration : IEntityTypeConfiguration<FleetVehicle>
{
    public void Configure(EntityTypeBuilder<FleetVehicle> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlateNumber).IsRequired().HasMaxLength(40);
        entity.Property(x => x.Model).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Color).HasMaxLength(80);
        entity.Property(x => x.Odometer).HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.PlateNumber).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class VehicleRequestConfigration : IEntityTypeConfiguration<VehicleRequest>
{
    public void Configure(EntityTypeBuilder<VehicleRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.RequesterName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Purpose).IsRequired().HasMaxLength(1000);
        entity.Property(x => x.DecisionNote).HasMaxLength(1000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.Status, x.RequestedFrom });
    }
}

public class VehicleAssignmentConfigration : IEntityTypeConfiguration<VehicleAssignment>
{
    public void Configure(EntityTypeBuilder<VehicleAssignment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EmployeeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.HandOdometer).HasMaxLength(80);
        entity.Property(x => x.ReceiveOdometer).HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.FleetVehicleId, x.Status });
        entity.HasOne(x => x.FleetVehicle).WithMany().HasForeignKey(x => x.FleetVehicleId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.VehicleRequest).WithMany().HasForeignKey(x => x.VehicleRequestId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class MaintenanceRequestConfigration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(200);
        entity.Property(x => x.AssetName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.IssueDescription).IsRequired().HasMaxLength(2000);
        entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
        entity.Property(x => x.ActualCost).HasPrecision(18, 2);
        entity.Property(x => x.VendorName).HasMaxLength(200);
        entity.Property(x => x.CompletionNotes).HasMaxLength(2000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.RequestType, x.Status });
        entity.HasOne(x => x.FleetVehicle).WithMany().HasForeignKey(x => x.FleetVehicleId).OnDelete(DeleteBehavior.SetNull);
    }
}
