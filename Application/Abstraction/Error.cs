namespace Application.Abstraction;

public record Error(string Code, string Description, int? StatuesCode)
{
    public static readonly Error None = new(string.Empty, string.Empty, null);
}
