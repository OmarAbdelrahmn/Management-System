using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class BoardMembershipConfigration : IEntityTypeConfiguration<BoardMembership>
{
    public void Configure(EntityTypeBuilder<BoardMembership> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        entity.Property(x => x.CumulativePercentage).HasPrecision(18, 2);
        entity.HasIndex(x => new { x.BoardId, x.UserId }).IsUnique();
        entity.HasOne(x => x.Board).WithMany(x => x.Memberships).HasForeignKey(x => x.BoardId).OnDelete(DeleteBehavior.Restrict);
    }
}
