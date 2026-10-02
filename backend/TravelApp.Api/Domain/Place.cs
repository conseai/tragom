using NetTopologySuite.Geometries;

namespace TravelApp.Api.Domain;

/// <summary>
/// A point of interest: a fortress, monastery, national park, accommodation, restaurant...
/// </summary>
public class Place
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Region { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Stored as PostGIS geography(Point, 4326): WGS84 lon/lat, distances in meters.
    /// NetTopologySuite convention: X = longitude, Y = latitude.
    /// </summary>
    public required Point Location { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
