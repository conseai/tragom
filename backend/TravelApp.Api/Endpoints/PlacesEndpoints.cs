using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using TravelApp.Api.Data;
using TravelApp.Api.Domain;

namespace TravelApp.Api.Endpoints;

public record PlaceDto(
    int Id, string Name, string Category, string Region, string Description,
    double Latitude, double Longitude, double? DistanceKm);

public record CreatePlaceRequest(
    string Name, string Category, string Region, string? Description,
    double Latitude, double Longitude);

public static class PlacesEndpoints
{
    private static readonly GeometryFactory Geo =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static IEndpointRouteBuilder MapPlacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/places").WithTags("Places");

        // GET /api/places
        group.MapGet("/", async (AppDbContext db) =>
        {
            var places = await db.Places.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            return places.Select(p => ToDto(p));
        });

        // GET /api/places/42
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Places.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id) is { } place
                ? Results.Ok(ToDto(place))
                : Results.NotFound());

        // GET /api/places/nearby?lat=45.2671&lng=19.8335&radiusKm=50
        // Translated by EF Core to PostGIS: ST_DWithin(...) for the filter, ST_Distance(...) for ordering.
        group.MapGet("/nearby", async (double lat, double lng, double? radiusKm, AppDbContext db) =>
        {
            if (!IsValidCoordinate(lat, lng))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["coordinates"] = ["lat must be between -90 and 90, lng between -180 and 180."]
                });

            var radiusMeters = Math.Clamp(radiusKm ?? 25, 0.1, 1000) * 1000;
            var origin = Geo.CreatePoint(new Coordinate(lng, lat));

            var rows = await db.Places.AsNoTracking()
                .Where(p => p.Location.IsWithinDistance(origin, radiusMeters))
                .OrderBy(p => p.Location.Distance(origin))
                .Select(p => new { Place = p, Meters = p.Location.Distance(origin) })
                .Take(100)
                .ToListAsync();

            return Results.Ok(rows.Select(r => ToDto(r.Place, Math.Round(r.Meters / 1000, 1))));
        });

        // POST /api/places
        group.MapPost("/", async (CreatePlaceRequest req, AppDbContext db) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(req.Name)) errors["name"] = ["Name is required."];
            if (string.IsNullOrWhiteSpace(req.Category)) errors["category"] = ["Category is required."];
            if (string.IsNullOrWhiteSpace(req.Region)) errors["region"] = ["Region is required."];
            if (!IsValidCoordinate(req.Latitude, req.Longitude))
                errors["coordinates"] = ["Latitude must be between -90 and 90, longitude between -180 and 180."];
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var place = new Place
            {
                Name = req.Name.Trim(),
                Category = req.Category.Trim(),
                Region = req.Region.Trim(),
                Description = req.Description?.Trim() ?? string.Empty,
                Location = Geo.CreatePoint(new Coordinate(req.Longitude, req.Latitude)),
            };
            db.Places.Add(place);
            await db.SaveChangesAsync();

            return Results.Created($"/api/places/{place.Id}", ToDto(place));
        });

        return app;
    }

    private static bool IsValidCoordinate(double lat, double lng) =>
        lat is >= -90 and <= 90 && lng is >= -180 and <= 180;

    // NetTopologySuite: Y = latitude, X = longitude
    private static PlaceDto ToDto(Place p, double? distanceKm = null) =>
        new(p.Id, p.Name, p.Category, p.Region, p.Description, p.Location.Y, p.Location.X, distanceKm);
}
