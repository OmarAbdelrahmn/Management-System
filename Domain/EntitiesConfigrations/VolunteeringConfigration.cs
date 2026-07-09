using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class VolunteerUserConfigration : IEntityTypeConfiguration<VolunteerUser>
{
    public void Configure(EntityTypeBuilder<VolunteerUser> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.VolunteerNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(40);
        entity.Property(x => x.Mobile).IsRequired().HasMaxLength(40);
        entity.Property(x => x.Email).HasMaxLength(160);
        entity.Property(x => x.Skills).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.VolunteerNumber).IsUnique();
        entity.HasIndex(x => new { x.Status, x.FullName });
    }
}

public class VolunteerRequestConfigration : IEntityTypeConfiguration<VolunteerRequest>
{
    public void Configure(EntityTypeBuilder<VolunteerRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.ApplicantName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Mobile).IsRequired().HasMaxLength(40);
        entity.Property(x => x.OpportunityTitle).HasMaxLength(250);
        entity.Property(x => x.DecisionNote).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.Source, x.Status });
        entity.HasOne(x => x.VolunteerUser).WithMany(x => x.Requests).HasForeignKey(x => x.VolunteerUserId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.VolunteerOpportunity).WithMany(x => x.Requests).HasForeignKey(x => x.VolunteerOpportunityId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class VolunteerOpportunityConfigration : IEntityTypeConfiguration<VolunteerOpportunity>
{
    public void Configure(EntityTypeBuilder<VolunteerOpportunity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.OpportunityNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Description).HasMaxLength(1500);
        entity.Property(x => x.Department).HasMaxLength(160);
        entity.Property(x => x.ProcedureNotes).HasMaxLength(1500);
        entity.Property(x => x.ReportSummary).HasMaxLength(2000);
        entity.HasIndex(x => x.OpportunityNumber).IsUnique();
        entity.HasIndex(x => new { x.Status, x.StartDate });
    }
}

public class VolunteerOpportunityTaskConfigration : IEntityTypeConfiguration<VolunteerOpportunityTask>
{
    public void Configure(EntityTypeBuilder<VolunteerOpportunityTask> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.AssignedTo).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.VolunteerOpportunityId, x.Status });
        entity.HasOne(x => x.VolunteerOpportunity).WithMany(x => x.Tasks).HasForeignKey(x => x.VolunteerOpportunityId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class VolunteerAttendanceRecordConfigration : IEntityTypeConfiguration<VolunteerAttendanceRecord>
{
    public void Configure(EntityTypeBuilder<VolunteerAttendanceRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Hours).HasPrecision(8, 2);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.VolunteerOpportunityId, x.VolunteerUserId, x.AttendanceDate });
        entity.HasOne(x => x.VolunteerOpportunity).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.VolunteerOpportunityId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.VolunteerUser).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.VolunteerUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
