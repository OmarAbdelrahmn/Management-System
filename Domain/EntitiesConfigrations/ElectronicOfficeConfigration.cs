using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class OfficeAttendanceEntryConfigration : IEntityTypeConfiguration<OfficeAttendanceEntry>
{
    public void Configure(EntityTypeBuilder<OfficeAttendanceEntry> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EmployeeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeName, x.AttendanceAt });
    }
}

public class OfficeReminderConfigration : IEntityTypeConfiguration<OfficeReminder>
{
    public void Configure(EntityTypeBuilder<OfficeReminder> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.OwnerName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.DueAt });
    }
}

public class OfficeAdministrativeRequestConfigration : IEntityTypeConfiguration<OfficeAdministrativeRequest>
{
    public void Configure(EntityTypeBuilder<OfficeAdministrativeRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.RequestNumber).IsUnique();
        entity.HasIndex(x => new { x.RequestType, x.Status });
    }
}

public class OfficeTransactionConfigration : IEntityTypeConfiguration<OfficeTransaction>
{
    public void Configure(EntityTypeBuilder<OfficeTransaction> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TransactionNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(250);
        entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(200);
        entity.Property(x => x.CurrentStep).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.TransactionNumber).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class OfficeLogRecordConfigration : IEntityTypeConfiguration<OfficeLogRecord>
{
    public void Configure(EntityTypeBuilder<OfficeLogRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Reference).HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.LogType, x.RecordDate });
    }
}
