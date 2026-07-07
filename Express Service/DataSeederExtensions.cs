namespace Express_Service;

public static class DataSeederExtensions
{
    public static async Task SeedDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
