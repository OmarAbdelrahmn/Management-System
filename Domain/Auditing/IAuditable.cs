namespace Domain.Auditing;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedByUserId { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedByUserId { get; set; }
}
