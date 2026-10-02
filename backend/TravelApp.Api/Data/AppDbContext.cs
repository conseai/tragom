using Microsoft.EntityFrameworkCore;
using TravelApp.Api.Domain;

namespace TravelApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Place> Places => Set<Place>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Makes the first migration run "CREATE EXTENSION IF NOT EXISTS postgis"
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Place>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Category).HasMaxLength(50);
            e.Property(p => p.Region).HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(2000);

            // geography (not geometry) so that distance functions work in meters on the globe
            e.Property(p => p.Location).HasColumnType("geography (point, 4326)");

            // Spatial index: makes "within X km" queries fast once you have thousands of places
            e.HasIndex(p => p.Location).HasMethod("gist");
        });
    }
}
