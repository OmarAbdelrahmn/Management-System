using Domain;
using Microsoft.EntityFrameworkCore;

namespace ManagementSystem.Tests;

internal static class ServiceTestFactory
{
    public static ApplicationDbcontext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbcontext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbcontext(options);
    }
}
