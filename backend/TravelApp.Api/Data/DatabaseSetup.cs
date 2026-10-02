using Microsoft.EntityFrameworkCore;

namespace TravelApp.Api.Data;

public static class DatabaseSetup
{
    /// <summary>
    /// Development only: applies pending EF Core migrations and inserts sample data into an empty database.
    /// </summary>
    public static async Task MigrateAndSeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSetup");

        if (!db.Database.GetMigrations().Any())
        {
            log.LogWarning(
                "No EF Core migrations exist yet, so the database has no tables. " +
                "Create the first one with: ./scripts/add-migration.sh InitialCreate");
            return;
        }

        await db.Database.MigrateAsync();

        if (!await db.Places.AnyAsync())
        {
            var places = SeedData.Places().ToList();
            db.Places.AddRange(places);
            await db.SaveChangesAsync();
            log.LogInformation("Seeded {Count} sample places", places.Count);
        }
    }
}
