using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class ProgramProjectConfigration : IEntityTypeConfiguration<ProgramProject>
{
    public void Configure(EntityTypeBuilder<ProgramProject> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ProjectCode).IsRequired().HasMaxLength(40);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ProjectType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Description).HasMaxLength(2000);
        entity.Property(x => x.ManagerName).HasMaxLength(200);
        entity.Property(x => x.Budget).HasColumnType("decimal(18,2)");
        entity.Property(x => x.TargetBeneficiaries).HasColumnType("decimal(18,2)");
        entity.Property(x => x.RegistrationFormJson).HasMaxLength(4000);
        entity.Property(x => x.SpecialProgramCategory).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ProjectCode).IsUnique();
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.IsPublished);
    }
}

public class ProgramProjectTaskConfigration : IEntityTypeConfiguration<ProgramProjectTask>
{
    public void Configure(EntityTypeBuilder<ProgramProjectTask> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.OwnerName).HasMaxLength(200);
        entity.Property(x => x.ProgressPercent).HasColumnType("decimal(5,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.Status });
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Tasks).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramProjectMilestoneConfigration : IEntityTypeConfiguration<ProgramProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProgramProjectMilestone> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ProgressPercent).HasColumnType("decimal(5,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Milestones).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramProjectContractConfigration : IEntityTypeConfiguration<ProgramProjectContract>
{
    public void Configure(EntityTypeBuilder<ProgramProjectContract> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ContractNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ContractNumber).IsUnique();
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Contracts).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.ProgramSupplier).WithMany(x => x.Contracts).HasForeignKey(x => x.ProgramSupplierId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgramProjectFinanceEntryConfigration : IEntityTypeConfiguration<ProgramProjectFinanceEntry>
{
    public void Configure(EntityTypeBuilder<ProgramProjectFinanceEntry> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.SourceOrPayee).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.EntryType, x.EntryDate });
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.FinanceEntries).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramProjectAssignmentConfigration : IEntityTypeConfiguration<ProgramProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProgramProjectAssignment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ExternalReference).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.AssignmentType });
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Assignments).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramProjectReportConfigration : IEntityTypeConfiguration<ProgramProjectReport>
{
    public void Configure(EntityTypeBuilder<ProgramProjectReport> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReportType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Summary).IsRequired().HasMaxLength(3000);
        entity.Property(x => x.FilePath).HasMaxLength(500);
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Reports).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramProjectActivityConfigration : IEntityTypeConfiguration<ProgramProjectActivity>
{
    public void Configure(EntityTypeBuilder<ProgramProjectActivity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Note).HasMaxLength(2000);
        entity.Property(x => x.FromStatus).HasMaxLength(80);
        entity.Property(x => x.ToStatus).HasMaxLength(80);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Reference).HasMaxLength(160);
        entity.HasIndex(x => new { x.ProgramProjectId, x.OccurredAt });
        entity.HasIndex(x => x.Type);
        entity.HasOne(x => x.ProgramProject).WithMany(x => x.Activities).HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramSupplierConfigration : IEntityTypeConfiguration<ProgramSupplier>
{
    public void Configure(EntityTypeBuilder<ProgramSupplier> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ContactPerson).HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.City).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Name).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class ProgramSupplierProposalConfigration : IEntityTypeConfiguration<ProgramSupplierProposal>
{
    public void Configure(EntityTypeBuilder<ProgramSupplierProposal> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ProposalNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Scope).HasMaxLength(3000);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ProposalNumber).IsUnique();
        entity.HasIndex(x => new { x.ProgramProjectId, x.Status });
        entity.HasIndex(x => new { x.ProgramSupplierId, x.Status });

        entity.HasOne(x => x.ProgramProject)
            .WithMany(x => x.SupplierProposals)
            .HasForeignKey(x => x.ProgramProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.ProgramSupplier)
            .WithMany(x => x.Proposals)
            .HasForeignKey(x => x.ProgramSupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ConvertedContract)
            .WithMany()
            .HasForeignKey(x => x.ConvertedContractId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProgramIdeaConfigration : IEntityTypeConfiguration<ProgramIdea>
{
    public void Configure(EntityTypeBuilder<ProgramIdea> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.OwnerName).HasMaxLength(200);
        entity.Property(x => x.Description).IsRequired().HasMaxLength(3000);
        entity.Property(x => x.MarketingNotes).HasMaxLength(2000);
        entity.Property(x => x.EstimatedBudget).HasColumnType("decimal(18,2)");
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.ConvertedProjectId);
        entity.HasOne(x => x.ConvertedProject).WithMany().HasForeignKey(x => x.ConvertedProjectId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgramApprovalConfigration : IEntityTypeConfiguration<ProgramApproval>
{
    public void Configure(EntityTypeBuilder<ProgramApproval> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ApprovalType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramIdeaId, x.Status });
        entity.HasOne(x => x.ProgramIdea).WithMany(x => x.Approvals).HasForeignKey(x => x.ProgramIdeaId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgramRegistrationConfigration : IEntityTypeConfiguration<ProgramRegistration>
{
    public void Configure(EntityTypeBuilder<ProgramRegistration> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ParticipantName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Mobile).HasMaxLength(30);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.ExternalReference).HasMaxLength(120);
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.Status });
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramSessionConfigration : IEntityTypeConfiguration<ProgramSession>
{
    public void Configure(EntityTypeBuilder<ProgramSession> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Location).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.StartsAt });
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramSessionAttendanceConfigration : IEntityTypeConfiguration<ProgramSessionAttendance>
{
    public void Configure(EntityTypeBuilder<ProgramSessionAttendance> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ParticipantName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ExternalReference).HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramSessionId, x.Status });
        entity.HasOne(x => x.ProgramSession).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.ProgramSessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramSurveyConfigration : IEntityTypeConfiguration<ProgramSurvey>
{
    public void Configure(EntityTypeBuilder<ProgramSurvey> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.QuestionsJson).IsRequired().HasMaxLength(4000);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.Status });
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramSurveySubmissionConfigration : IEntityTypeConfiguration<ProgramSurveySubmission>
{
    public void Configure(EntityTypeBuilder<ProgramSurveySubmission> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RespondentName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.AnswersJson).IsRequired().HasMaxLength(4000);
        entity.HasOne(x => x.ProgramSurvey).WithMany(x => x.Submissions).HasForeignKey(x => x.ProgramSurveyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramCertificateTemplateConfigration : IEntityTypeConfiguration<ProgramCertificateTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramCertificateTemplate> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.BodyTemplate).IsRequired().HasMaxLength(4000);
        entity.HasIndex(x => new { x.ProgramProjectId, x.IsActive });
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgramCertificateIssueConfigration : IEntityTypeConfiguration<ProgramCertificateIssue>
{
    public void Configure(EntityTypeBuilder<ProgramCertificateIssue> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CertificateNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.RecipientName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.CertificateNumber).IsUnique();
        entity.HasIndex(x => new { x.ProgramProjectId, x.Status });
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.ProgramCertificateTemplate).WithMany(x => x.CertificateIssues).HasForeignKey(x => x.ProgramCertificateTemplateId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProgramQualificationCaseConfigration : IEntityTypeConfiguration<ProgramQualificationCase>
{
    public void Configure(EntityTypeBuilder<ProgramQualificationCase> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BeneficiaryName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.NeedSummary).IsRequired().HasMaxLength(3000);
        entity.Property(x => x.ManagementOpinion).HasMaxLength(2000);
        entity.Property(x => x.ApprovedAmount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
        entity.HasOne(x => x.ProgramProject).WithMany().HasForeignKey(x => x.ProgramProjectId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgramQualificationInstallmentConfigration : IEntityTypeConfiguration<ProgramQualificationInstallment>
{
    public void Configure(EntityTypeBuilder<ProgramQualificationInstallment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.ProgramQualificationCaseId, x.Status, x.DueDate });
        entity.HasOne(x => x.ProgramQualificationCase).WithMany(x => x.Installments).HasForeignKey(x => x.ProgramQualificationCaseId).OnDelete(DeleteBehavior.Cascade);
    }
}
