using Domain;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Express_Service;

public class DataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbcontext dbcontext,
    IOptions<SeedOptions> options,
    ILogger<DataSeeder> logger)
{
    private static readonly string[] Roles =
    [
        "Admin",
        "BoardSecretary",
        "BoardMember",
        "BoardChairman"
    ];

    private readonly SeedOptions seedOptions = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        var admin = await SeedAdminUserAsync();

        if (seedOptions.CreateDemoBoard && admin is not null)
            await SeedDemoBoardAsync(admin, cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole { Name = role });
            if (result.Succeeded)
                logger.LogInformation("Seeded role {Role}", role);
            else
                logger.LogWarning("Failed to seed role {Role}: {Errors}", role, string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }

    private async Task<ApplicationUser?> SeedAdminUserAsync()
    {
        if (string.IsNullOrWhiteSpace(seedOptions.AdminEmail) || string.IsNullOrWhiteSpace(seedOptions.AdminPassword))
        {
            logger.LogInformation("Admin seed skipped because Seed:AdminEmail or Seed:AdminPassword is empty.");
            return null;
        }

        var admin = await userManager.FindByEmailAsync(seedOptions.AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = seedOptions.AdminEmail,
                Email = seedOptions.AdminEmail,
                FullName = seedOptions.AdminFullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, seedOptions.AdminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("Failed to seed admin user {Email}: {Errors}", seedOptions.AdminEmail, string.Join("; ", createResult.Errors.Select(x => x.Description)));
                return null;
            }

            logger.LogInformation("Seeded admin user {Email}", seedOptions.AdminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");

        return admin;
    }

    private async Task SeedDemoBoardAsync(ApplicationUser admin, CancellationToken cancellationToken)
    {
        if (await dbcontext.Boards.AnyAsync(x => x.Code == "BD", cancellationToken))
            return;

        var now = DateTime.UtcNow.AddHours(3);
        var cycleStart = new DateTime(now.Year, 1, 1);
        var cycleEnd = new DateTime(now.Year, 12, 31, 23, 59, 59);

        var board = new Board
        {
            Name = "مجلس الإدارة التجريبي",
            Code = "BD",
            Cycles =
            {
                new BoardCycle
                {
                    CycleNumber = 1,
                    ConsecutiveCycleCount = 1,
                    StartsAt = cycleStart,
                    EndsAt = cycleEnd
                }
            },
            Memberships =
            {
                new BoardMembership
                {
                    UserId = admin.Id,
                    HasPaidFees = true,
                    IsChairman = true,
                    IsSecretary = true,
                    IsActive = true
                }
            }
        };

        dbcontext.Boards.Add(board);
        await dbcontext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded demo board {Code}", board.Code);
    }
}
