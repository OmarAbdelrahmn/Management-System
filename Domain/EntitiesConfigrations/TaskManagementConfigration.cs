using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class ManagementTaskConfigration : IEntityTypeConfiguration<ManagementTask>
{
    public void Configure(EntityTypeBuilder<ManagementTask> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(4000);
        entity.Property(x => x.CreatorUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.AssigneeUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.RelatedEntityType).HasMaxLength(80);
        entity.Property(x => x.CompletionNote).HasMaxLength(2000);
        entity.Property(x => x.RedirectReason).HasMaxLength(1000);
        entity.Property(x => x.DeletedReason).HasMaxLength(1000);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.AssigneeUserId);
        entity.HasIndex(x => x.DueAt);

        entity.HasOne(x => x.CreatorUser)
            .WithMany()
            .HasForeignKey(x => x.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.AssigneeUser)
            .WithMany()
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApprovalRouteConfigration : IEntityTypeConfiguration<ApprovalRoute>
{
    public void Configure(EntityTypeBuilder<ApprovalRoute> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(160);
        entity.Property(x => x.EntityType).IsRequired().HasMaxLength(80);
        entity.HasIndex(x => new { x.EntityType, x.NameAr }).IsUnique();
    }
}

public class ManagementTaskActivityConfigration : IEntityTypeConfiguration<ManagementTaskActivity>
{
    public void Configure(EntityTypeBuilder<ManagementTaskActivity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ActorUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Note).HasMaxLength(2000);
        entity.Property(x => x.FromAssigneeUserId).HasMaxLength(450);
        entity.Property(x => x.ToAssigneeUserId).HasMaxLength(450);
        entity.HasIndex(x => new { x.ManagementTaskId, x.ActionAt });
        entity.HasIndex(x => new { x.Type, x.ActionAt });

        entity.HasOne(x => x.ManagementTask)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.ManagementTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.FromAssigneeUser)
            .WithMany()
            .HasForeignKey(x => x.FromAssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ToAssigneeUser)
            .WithMany()
            .HasForeignKey(x => x.ToAssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApprovalStepConfigration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(160);
        entity.Property(x => x.ApproverUserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.ApprovalRouteId, x.StepOrder }).IsUnique();

        entity.HasOne(x => x.ApprovalRoute)
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.ApprovalRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.ApproverUser)
            .WithMany()
            .HasForeignKey(x => x.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApprovalRequestConfigration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceType).IsRequired().HasMaxLength(80);
        entity.Property(x => x.RequestedByUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.FinalComment).HasMaxLength(2000);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.RequestedByUserId);

        entity.HasOne(x => x.ApprovalRoute)
            .WithMany()
            .HasForeignKey(x => x.ApprovalRouteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApprovalActionConfigration : IEntityTypeConfiguration<ApprovalAction>
{
    public void Configure(EntityTypeBuilder<ApprovalAction> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ActionByUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Comment).HasMaxLength(2000);

        entity.HasOne(x => x.ApprovalRequest)
            .WithMany(x => x.Actions)
            .HasForeignKey(x => x.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.ActionByUser)
            .WithMany()
            .HasForeignKey(x => x.ActionByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
