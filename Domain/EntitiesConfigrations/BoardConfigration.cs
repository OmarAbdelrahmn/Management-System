using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class BoardConfigration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(20);
        entity.HasIndex(x => x.Code).IsUnique();
    }
}
