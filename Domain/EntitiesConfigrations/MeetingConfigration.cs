using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class MeetingConfigration :
    IEntityTypeConfiguration<BoardMeeting>,
    IEntityTypeConfiguration<MeetingAgendaItem>,
    IEntityTypeConfiguration<MeetingInvitation>,
    IEntityTypeConfiguration<MeetingNote>,
    IEntityTypeConfiguration<MeetingManager>,
    IEntityTypeConfiguration<MeetingCandidate>,
    IEntityTypeConfiguration<MeetingGuest>,
    IEntityTypeConfiguration<MeetingAttachment>,
    IEntityTypeConfiguration<MeetingImage>,
    IEntityTypeConfiguration<MeetingApproval>,
    IEntityTypeConfiguration<MeetingRepeatDraft>
{
    public void Configure(EntityTypeBuilder<BoardMeeting> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Category).HasMaxLength(150);
        entity.Property(x => x.Platform).HasMaxLength(150);
        entity.Property(x => x.Location).HasMaxLength(250);
        entity.Property(x => x.Notes).HasMaxLength(4000);
        entity.Property(x => x.HasVoting).HasDefaultValue(true);
        entity.Property(x => x.Type).HasDefaultValue(MeetingType.General);
        entity.Property(x => x.Importance).HasDefaultValue(MeetingImportance.Normal);
        entity.Property(x => x.RepeatMode).HasDefaultValue(MeetingRepeatMode.None);
        entity.Property(x => x.MinimumAttendancePercentage).HasPrecision(5, 2).HasDefaultValue(100m);
        entity.HasOne(x => x.BoardCycle).WithMany(x => x.Meetings).HasForeignKey(x => x.BoardCycleId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingAgendaItem> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Description).HasMaxLength(4000);
        entity.Property(x => x.RejectionText).HasMaxLength(1000);
        entity.HasIndex(x => new { x.BoardMeetingId, x.ItemNumber }).IsUnique();
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.AgendaItems).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingInvitation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MemberUserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.BoardMeetingId, x.MemberUserId }).IsUnique();
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Invitations).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingNote> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Text).IsRequired().HasMaxLength(2000);
        entity.HasOne(x => x.MeetingInvitation).WithMany(x => x.Notes).HasForeignKey(x => x.MeetingInvitationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.MeetingAgendaItem).WithMany().HasForeignKey(x => x.MeetingAgendaItemId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingManager> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.BoardMeetingId, x.UserId }).IsUnique();
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Managers).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingCandidate> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        entity.HasIndex(x => new { x.BoardMeetingId, x.UserId }).IsUnique();
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Candidates).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingGuest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(250);
        entity.Property(x => x.Email).HasMaxLength(320);
        entity.Property(x => x.PhoneNumber).HasMaxLength(50);
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Guests).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingAttachment> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        entity.Property(x => x.ContentType).HasMaxLength(150);
        entity.Property(x => x.StoragePath).IsRequired().HasMaxLength(1000);
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Attachments).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingImage> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        entity.Property(x => x.ContentType).HasMaxLength(150);
        entity.Property(x => x.StoragePath).IsRequired().HasMaxLength(1000);
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Images).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingApproval> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ApproverUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Comments).HasMaxLength(1000);
        entity.HasOne(x => x.BoardMeeting).WithMany(x => x.Approvals).HasForeignKey(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingRepeatDraft> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasOne(x => x.SourceBoardMeeting).WithMany(x => x.RepeatDrafts).HasForeignKey(x => x.SourceBoardMeetingId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CreatedBoardMeeting).WithMany().HasForeignKey(x => x.CreatedBoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }
}
