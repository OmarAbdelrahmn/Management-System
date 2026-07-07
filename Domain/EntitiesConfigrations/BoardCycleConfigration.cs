using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class BoardCycleConfigration : IEntityTypeConfiguration<BoardCycle>
{
    public void Configure(EntityTypeBuilder<BoardCycle> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.BoardId, x.CycleNumber }).IsUnique();
        entity.HasOne(x => x.Board).WithMany(x => x.Cycles).HasForeignKey(x => x.BoardId).OnDelete(DeleteBehavior.Restrict);
    }
}
