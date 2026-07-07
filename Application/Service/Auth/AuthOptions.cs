namespace Application.Service.Auth;

public class AuthOptions
{
    public string Key { get; set; } = "development-key-change-before-production-development-key";
    public string Issuer { get; set; } = "ManagementSystem";
    public string Audience { get; set; } = "ManagementSystemClients";
    public int ExpirationMinutes { get; set; } = 120;
}
