using Domain.Auditing;

namespace Domain.Entities;

public class BoardMembership : IAuditable
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool HasPaidFees { get; set; }
    public bool IsSupportingMember { get; set; }
    public decimal CumulativePercentage { get; set; }
    public bool IsChairman { get; set; }
    public bool IsSecretary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Board? Board { get; set; }
}
