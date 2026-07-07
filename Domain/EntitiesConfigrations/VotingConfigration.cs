using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class VotingConfigration :
    IEntityTypeConfiguration<VoteSession>,
    IEntityTypeConfiguration<Vote>,
    IEntityTypeConfiguration<Decision>,
    IEntityTypeConfiguration<MeetingMinute>
{
    public void Configure(EntityTypeBuilder<VoteSession> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.MeetingAgendaItemId).IsUnique();
        entity.HasOne(x => x.MeetingAgendaItem).WithOne(x => x.VoteSession).HasForeignKey<VoteSession>(x => x.MeetingAgendaItemId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Vote> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MemberUserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.Weight).HasPrecision(18, 2);
        entity.Property(x => x.RejectionReason).HasMaxLength(1000);
        entity.HasIndex(x => new { x.VoteSessionId, x.MemberUserId }).IsUnique();
        entity.HasOne(x => x.VoteSession).WithMany(x => x.Votes).HasForeignKey(x => x.VoteSessionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Decision> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(50);
        entity.Property(x => x.SignedByUserId).HasMaxLength(450);
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasOne(x => x.MeetingAgendaItem).WithOne(x => x.Decision).HasForeignKey<Decision>(x => x.MeetingAgendaItemId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MeetingMinute> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DraftText).HasMaxLength(8000);
        entity.Property(x => x.PdfPath).HasMaxLength(500);
        entity.HasIndex(x => x.BoardMeetingId).IsUnique();
        entity.HasOne(x => x.BoardMeeting).WithOne(x => x.Minute).HasForeignKey<MeetingMinute>(x => x.BoardMeetingId).OnDelete(DeleteBehavior.Restrict);
    }
}
