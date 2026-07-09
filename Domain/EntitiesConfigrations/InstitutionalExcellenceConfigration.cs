using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class PerformanceMeasureConfigration : IEntityTypeConfiguration<PerformanceMeasure>
{
    public void Configure(EntityTypeBuilder<PerformanceMeasure> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.TargetValue).HasPrecision(18, 2);
        entity.Property(x => x.ActualValue).HasPrecision(18, 2);
        entity.Property(x => x.Unit).IsRequired().HasMaxLength(40);
        entity.Property(x => x.ReportingPeriod).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.MeasureType, x.Status });
        entity.HasIndex(x => x.Code);
    }
}

public class GovernanceCycleConfigration : IEntityTypeConfiguration<GovernanceCycle>
{
    public void Configure(EntityTypeBuilder<GovernanceCycle> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.RoadmapNotes).HasMaxLength(2000);
        entity.HasIndex(x => new { x.Year, x.Status });
        entity.HasIndex(x => x.IsActive);
    }
}

public class GovernanceCriterionConfigration : IEntityTypeConfiguration<GovernanceCriterion>
{
    public void Configure(EntityTypeBuilder<GovernanceCriterion> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Weight).HasPrecision(18, 2);
        entity.Property(x => x.TargetScore).HasPrecision(18, 2);
        entity.Property(x => x.ActualScore).HasPrecision(18, 2);
        entity.Property(x => x.Answer).HasMaxLength(4000);
        entity.Property(x => x.VerificationNotes).HasMaxLength(2000);
        entity.Property(x => x.FinancialIndicatorValue).HasPrecision(18, 2);
        entity.HasIndex(x => new { x.GovernanceCycleId, x.Status });
        entity.HasOne(x => x.GovernanceCycle).WithMany(x => x.Criteria).HasForeignKey(x => x.GovernanceCycleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GovernanceAttachmentConfigration : IEntityTypeConfiguration<GovernanceAttachment>
{
    public void Configure(EntityTypeBuilder<GovernanceAttachment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FileName).IsRequired().HasMaxLength(250);
        entity.Property(x => x.FileUrl).IsRequired().HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.UploadedAt);
        entity.HasOne(x => x.GovernanceCriterion).WithMany(x => x.Attachments).HasForeignKey(x => x.GovernanceCriterionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GovernanceTaskConfigration : IEntityTypeConfiguration<GovernanceTask>
{
    public void Configure(EntityTypeBuilder<GovernanceTask> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.OwnerName).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Status, x.DueDate });
        entity.HasOne(x => x.GovernanceCycle).WithMany(x => x.Tasks).HasForeignKey(x => x.GovernanceCycleId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class StrategicPlanConfigration : IEntityTypeConfiguration<StrategicPlan>
{
    public void Configure(EntityTypeBuilder<StrategicPlan> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Vision).HasMaxLength(2000);
        entity.Property(x => x.Mission).HasMaxLength(2000);
        entity.Property(x => x.Notes).HasMaxLength(2000);
        entity.HasIndex(x => new { x.Status, x.StartDate, x.EndDate });
    }
}

public class StrategicPerspectiveConfigration : IEntityTypeConfiguration<StrategicPerspective>
{
    public void Configure(EntityTypeBuilder<StrategicPerspective> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.HasIndex(x => new { x.StrategicPlanId, x.SortOrder });
        entity.HasOne(x => x.StrategicPlan).WithMany(x => x.Perspectives).HasForeignKey(x => x.StrategicPlanId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategicGoalConfigration : IEntityTypeConfiguration<StrategicGoal>
{
    public void Configure(EntityTypeBuilder<StrategicGoal> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Description).HasMaxLength(2000);
        entity.Property(x => x.Vision2030Alignment).HasMaxLength(500);
        entity.Property(x => x.SustainabilityAlignment).HasMaxLength(500);
        entity.HasIndex(x => new { x.StrategicPerspectiveId, x.SortOrder });
        entity.HasOne(x => x.StrategicPerspective).WithMany(x => x.Goals).HasForeignKey(x => x.StrategicPerspectiveId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategicIndicatorConfigration : IEntityTypeConfiguration<StrategicIndicator>
{
    public void Configure(EntityTypeBuilder<StrategicIndicator> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
        entity.Property(x => x.TargetValue).HasPrecision(18, 2);
        entity.Property(x => x.ActualValue).HasPrecision(18, 2);
        entity.Property(x => x.Unit).IsRequired().HasMaxLength(40);
        entity.Property(x => x.OwnerName).HasMaxLength(200);
        entity.Property(x => x.RelatedProjectName).HasMaxLength(250);
        entity.Property(x => x.RelatedProgramName).HasMaxLength(250);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.StrategicPlanId, x.Kind, x.Status });
        entity.HasOne(x => x.StrategicPlan).WithMany(x => x.Indicators).HasForeignKey(x => x.StrategicPlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.StrategicGoal).WithMany(x => x.Indicators).HasForeignKey(x => x.StrategicGoalId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(x => x.ParentIndicator).WithMany(x => x.SubIndicators).HasForeignKey(x => x.ParentIndicatorId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class StrategicVariableConfigration : IEntityTypeConfiguration<StrategicVariable>
{
    public void Configure(EntityTypeBuilder<StrategicVariable> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Value).HasPrecision(18, 2);
        entity.Property(x => x.Source).HasMaxLength(300);
        entity.HasIndex(x => new { x.StrategicPlanId, x.IsAutomated });
        entity.HasOne(x => x.StrategicPlan).WithMany(x => x.Variables).HasForeignKey(x => x.StrategicPlanId).OnDelete(DeleteBehavior.Cascade);
    }
}
