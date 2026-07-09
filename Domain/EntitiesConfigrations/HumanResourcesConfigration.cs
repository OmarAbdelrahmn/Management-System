using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class EmployeeDepartmentConfigration : IEntityTypeConfiguration<EmployeeDepartment>
{
    public void Configure(EntityTypeBuilder<EmployeeDepartment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameEn).HasMaxLength(120);
        entity.HasIndex(x => x.NameAr).IsUnique();
    }
}

public class JobTitleConfigration : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(120);
        entity.Property(x => x.NameEn).HasMaxLength(120);
        entity.HasIndex(x => x.NameAr).IsUnique();
    }
}

public class EmployeeProfileConfigration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EmployeeNumber).IsRequired().HasMaxLength(40);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NationalId).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Allowances).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.TerminationReason).HasMaxLength(1000);
        entity.HasIndex(x => x.EmployeeNumber).IsUnique();
        entity.HasIndex(x => x.NationalId);
        entity.HasIndex(x => x.Email);

        entity.HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.JobTitle)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeAttendanceConfigration : IEntityTypeConfiguration<EmployeeAttendance>
{
    public void Configure(EntityTypeBuilder<EmployeeAttendance> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.WorkDate }).IsUnique();

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeLeaveRequestConfigration : IEntityTypeConfiguration<EmployeeLeaveRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.LeaveType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Reason).HasMaxLength(1000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.Status });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeDocumentConfigration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.DocumentType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.FilePath).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.DocumentType });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeDisciplinaryRecordConfigration : IEntityTypeConfiguration<EmployeeDisciplinaryRecord>
{
    public void Configure(EntityTypeBuilder<EmployeeDisciplinaryRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        entity.Property(x => x.ActionTaken).HasMaxLength(1000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.Type, x.Status });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.DisciplinaryRecords)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeLeaveBalanceConfigration : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.LeaveType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.EntitledDays).HasColumnType("decimal(18,2)");
        entity.Property(x => x.UsedDays).HasColumnType("decimal(18,2)");
        entity.Property(x => x.CarriedDays).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.Year, x.LeaveType }).IsUnique();

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeEvaluationConfigration : IEntityTypeConfiguration<EmployeeEvaluation>
{
    public void Configure(EntityTypeBuilder<EmployeeEvaluation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Score).HasColumnType("decimal(18,2)");
        entity.Property(x => x.MaxScore).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Rating).IsRequired().HasMaxLength(80);
        entity.Property(x => x.EvaluatorName).HasMaxLength(200);
        entity.Property(x => x.Strengths).HasMaxLength(1000);
        entity.Property(x => x.ImprovementAreas).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.PeriodStart, x.PeriodEnd });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeCardIssueConfigration : IEntityTypeConfiguration<EmployeeCardIssue>
{
    public void Configure(EntityTypeBuilder<EmployeeCardIssue> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CardType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.CardNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.CardNumber).IsUnique();
        entity.HasIndex(x => new { x.EmployeeProfileId, x.CardType });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.CardIssues)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeLetterRequestConfigration : IEntityTypeConfiguration<EmployeeLetterRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeLetterRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.LetterType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Subject).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Purpose).HasMaxLength(500);
        entity.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.Status });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.LetterRequests)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EmployeePayrollRecordConfigration : IEntityTypeConfiguration<EmployeePayrollRecord>
{
    public void Configure(EntityTypeBuilder<EmployeePayrollRecord> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Allowances).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Deductions).HasColumnType("decimal(18,2)");
        entity.Property(x => x.NetSalary).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.PayrollMonth }).IsUnique();

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.PayrollRecords)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeAttendancePolicyConfigration : IEntityTypeConfiguration<EmployeeAttendancePolicy>
{
    public void Configure(EntityTypeBuilder<EmployeeAttendancePolicy> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
        entity.Property(x => x.WorkDays).IsRequired().HasMaxLength(200);
        entity.HasIndex(x => x.Name).IsUnique();
    }
}

public class EmployeeAttendanceLocationConfigration : IEntityTypeConfiguration<EmployeeAttendanceLocation>
{
    public void Configure(EntityTypeBuilder<EmployeeAttendanceLocation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
        entity.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Name).IsUnique();
    }
}

public class EmployeeOfficialVacationConfigration : IEntityTypeConfiguration<EmployeeOfficialVacation>
{
    public void Configure(EntityTypeBuilder<EmployeeOfficialVacation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.StartsAt, x.EndsAt });
    }
}

public class EmployeeAttendanceExcuseConfigration : IEntityTypeConfiguration<EmployeeAttendanceExcuse>
{
    public void Configure(EntityTypeBuilder<EmployeeAttendanceExcuse> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ExcuseType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.WorkDate, x.Status });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.AttendanceExcuses)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HrSafetyCategoryConfigration : IEntityTypeConfiguration<HrSafetyCategory>
{
    public void Configure(EntityTypeBuilder<HrSafetyCategory> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.HasIndex(x => x.Name).IsUnique();
    }
}

public class HrSafetyProcedureConfigration : IEntityTypeConfiguration<HrSafetyProcedure>
{
    public void Configure(EntityTypeBuilder<HrSafetyProcedure> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ProcedureText).IsRequired().HasMaxLength(2000);

        entity.HasOne(x => x.HrSafetyCategory)
            .WithMany(x => x.Procedures)
            .HasForeignKey(x => x.HrSafetyCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class HrSafetyInspectionConfigration : IEntityTypeConfiguration<HrSafetyInspection>
{
    public void Configure(EntityTypeBuilder<HrSafetyInspection> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Location).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        entity.Property(x => x.CorrectiveAction).HasMaxLength(2000);
        entity.HasIndex(x => new { x.InspectionDate, x.Status });

        entity.HasOne(x => x.HrSafetyCategory)
            .WithMany(x => x.Inspections)
            .HasForeignKey(x => x.HrSafetyCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class RecruitmentRequestConfigration : IEntityTypeConfiguration<RecruitmentRequest>
{
    public void Configure(EntityTypeBuilder<RecruitmentRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestTitle).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Justification).HasMaxLength(1000);
        entity.Property(x => x.CandidateName).HasMaxLength(200);
        entity.Property(x => x.CandidateMobile).HasMaxLength(30);
        entity.Property(x => x.CandidateEmail).HasMaxLength(256);
        entity.Property(x => x.InterviewNotes).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);

        entity.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.JobTitle)
            .WithMany()
            .HasForeignKey(x => x.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeAdministrativeRequestConfigration : IEntityTypeConfiguration<EmployeeAdministrativeRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeAdministrativeRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Details).IsRequired().HasMaxLength(2000);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EmployeeProfileId, x.Status });

        entity.HasOne(x => x.EmployeeProfile)
            .WithMany(x => x.AdministrativeRequests)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
