using Domain.Auditing;

namespace Domain.Entities;

public class Board : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public BoardStatus Status { get; set; } = BoardStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<BoardCycle> Cycles { get; set; } = new List<BoardCycle>();
    public ICollection<BoardMembership> Memberships { get; set; } = new List<BoardMembership>();
}
