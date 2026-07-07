namespace Domain.Auditing;

public interface ICurrentUserContext
{
    string? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
}
