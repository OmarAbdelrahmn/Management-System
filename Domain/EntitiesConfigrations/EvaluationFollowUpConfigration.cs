using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class FollowUpCaseConfigration : IEntityTypeConfiguration<FollowUpCase>
{
    public void Configure(EntityTypeBuilder<FollowUpCase> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CaseNumber).IsRequired().HasMaxLength(80);
        entity.Property(x => x.SubjectName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(120);
        entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(200);
        entity.Property(x => x.RejectionNote).HasMaxLength(1500);
        entity.Property(x => x.CompletionSummary).HasMaxLength(2000);
        entity.Property(x => x.ApprovalNote).HasMaxLength(1500);
        entity.HasIndex(x => x.CaseNumber).IsUnique();
        entity.HasIndex(x => new { x.SubjectType, x.Status });
        entity.HasIndex(x => new { x.Status, x.RequestDate });
    }
}

public class FollowUpActivityConfigration : IEntityTypeConfiguration<FollowUpActivity>
{
    public void Configure(EntityTypeBuilder<FollowUpActivity> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SubjectName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(120);
        entity.Property(x => x.ActivityType).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Summary).IsRequired().HasMaxLength(3000);
        entity.Property(x => x.Result).HasMaxLength(2000);
        entity.Property(x => x.OwnerName).HasMaxLength(200);
        entity.HasIndex(x => new { x.SubjectType, x.ActivityDate });
        entity.HasIndex(x => x.RequiresNextAction);
        entity.HasOne(x => x.FollowUpCase).WithMany(x => x.Activities).HasForeignKey(x => x.FollowUpCaseId).OnDelete(DeleteBehavior.SetNull);
    }
}
