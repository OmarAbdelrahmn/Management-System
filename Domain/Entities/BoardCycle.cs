using Domain.Auditing;

namespace Domain.Entities;

public class BoardCycle : IAuditable
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public int CycleNumber { get; set; }
    public int ConsecutiveCycleCount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int NextDecisionSequence { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Board? Board { get; set; }
    public ICollection<BoardMeeting> Meetings { get; set; } = new List<BoardMeeting>();
}
