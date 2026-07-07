namespace Application.Contracts.Auth;

public record RegisterRequest(string FullName, string Email, string Password, string Role);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string UserId, string FullName, string Email, IEnumerable<string> Roles, string Token, DateTime ExpiresAt);
