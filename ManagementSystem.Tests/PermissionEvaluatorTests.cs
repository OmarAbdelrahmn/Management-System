using Application.Service.Permissions;
using Domain.Entities;
using Domain.Identity;
using System.Security.Claims;

namespace ManagementSystem.Tests;

public class PermissionEvaluatorTests
{
    [Fact]
    public async Task HasPermissionAsync_UsesRoleGrantAndHonorsAdminBypass()
    {
        await using var dbcontext = ServiceTestFactory.CreateDbContext();
        var role = new ApplicationRole { Id = "role-operator", Name = "Operator" };
        var permission = new AppPermission { Key = "system.hr.employee", NameAr = "الموظفون", Category = "HR" };
        dbcontext.AddRange(role, permission); await dbcontext.SaveChangesAsync();
        dbcontext.RolePermissions.Add(new RolePermission { RoleId = role.Id, AppPermissionId = permission.Id, IsGranted = true }); await dbcontext.SaveChangesAsync();
        var service = new PermissionEvaluator(dbcontext);
        var operatorPrincipal = Principal("Operator");

        Assert.True(await service.HasPermissionAsync(operatorPrincipal, permission.Key));
        Assert.False(await service.HasPermissionAsync(operatorPrincipal, "system.hr.payroll"));
        Assert.True(await service.HasPermissionAsync(Principal("Admin"), "anything"));
        Assert.True(await service.HasPermissionPrefixAsync(operatorPrincipal, "system.hr."));
        Assert.False(await service.HasPermissionPrefixAsync(operatorPrincipal, "system.accounting."));
        Assert.True(await service.HasPermissionPrefixAsync(Principal("Admin"), "system.anything."));
    }

    private static ClaimsPrincipal Principal(string role) => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user"), new Claim(ClaimTypes.Role, role)], "test"));
}
