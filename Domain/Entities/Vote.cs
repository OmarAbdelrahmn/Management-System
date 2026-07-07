using Domain.Auditing;

namespace Domain.Entities;

public class Vote : IAuditable
{
    public int Id { get; set; }
    public int VoteSessionId { get; set; }
    public string MemberUserId { get; set; } = string.Empty;
    public VoteChoice Choice { get; set; }
    public decimal Weight { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public VoteSession? VoteSession { get; set; }
}
