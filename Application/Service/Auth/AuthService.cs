using Application.Abstraction;
using Application.Contracts.Auth;
using Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Service.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<AuthOptions> options) : IAuthService
{
    private readonly AuthOptions authOptions = options.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var role = NormalizeRole(request.Role);
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole { Name = role });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result.Failure<AuthResponse>(new Error("Auth.RegisterFailed", string.Join("; ", result.Errors.Select(x => x.Description)), StatusCodes.Status400BadRequest));

        await userManager.AddToRoleAsync(user, role);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid email or password.", StatusCodes.Status401Unauthorized));

        return await BuildAuthResponseAsync(user);
    }

    private async Task<Result<AuthResponse>> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(authOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Key));
        var token = new JwtSecurityToken(
            issuer: authOptions.Issuer,
            audience: authOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var tokenText = new JwtSecurityTokenHandler().WriteToken(token);
        return Result.Success(new AuthResponse(user.Id, user.FullName, user.Email ?? string.Empty, roles, tokenText, expiresAt));
    }

    private static string NormalizeRole(string role) =>
        role.Trim() switch
        {
            "BoardSecretary" => "BoardSecretary",
            "BoardMember" => "BoardMember",
            "BoardChairman" => "BoardChairman",
            "Admin" => "Admin",
            _ => "BoardMember"
        };
}
