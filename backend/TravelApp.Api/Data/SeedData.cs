using NetTopologySuite;
using NetTopologySuite.Geometries;
using TravelApp.Api.Domain;

namespace TravelApp.Api.Data;

/// <summary>
/// Sample data so every developer starts with the same realistic places.
/// Coordinates are approximate and intended for development only.
/// </summary>
public static class SeedData
{
    private static readonly GeometryFactory Geo =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private static Point At(double lat, double lng) => Geo.CreatePoint(new Coordinate(lng, lat));

    public static IEnumerable<Place> Places() =>
    [
        new() { Name = "Petrovaradin Fortress", Category = "Fortress", Region = "Novi Sad",
                Description = "Baroque fortress above the Danube with underground military galleries and the clock tower.",
                Location = At(45.2528, 19.8622) },
        new() { Name = "Sremski Karlovci", Category = "Town", Region = "Srem",
                Description = "Small baroque town known for its wine cellars, the Patriarchate and the Four Lions fountain.",
                Location = At(45.2025, 19.9339) },
        new() { Name = "Fruška Gora National Park", Category = "Nature", Region = "Srem",
                Description = "Low mountain with forest trails, vineyards and sixteen Orthodox monasteries.",
                Location = At(45.1553, 19.7106) },
        new() { Name = "Subotica City Hall", Category = "Architecture", Region = "North Bačka",
                Description = "Hungarian Art Nouveau landmark with a viewing tower over the city.",
                Location = At(46.1003, 19.6656) },
        new() { Name = "Belgrade Fortress (Kalemegdan)", Category = "Fortress", Region = "Belgrade",
                Description = "Fortress and park at the confluence of the Sava and the Danube.",
                Location = At(44.8229, 20.4506) },
        new() { Name = "Golubac Fortress", Category = "Fortress", Region = "Braničevo",
                Description = "Medieval fortress at the entrance to the Iron Gates gorge on the Danube.",
                Location = At(44.6619, 21.6322) },
        new() { Name = "Tara National Park", Category = "Nature", Region = "Zlatibor",
                Description = "Forested plateau with viewpoints over the Drina canyon and Perućac lake.",
                Location = At(43.8738, 19.4380) },
        new() { Name = "Zlatibor", Category = "Nature", Region = "Zlatibor",
                Description = "Mountain resort area for hiking in summer and skiing in winter.",
                Location = At(43.7290, 19.7000) },
        new() { Name = "Uvac Special Nature Reserve", Category = "Nature", Region = "Zlatibor",
                Description = "Meandering river canyon, home to a large griffon vulture colony.",
                Location = At(43.3950, 19.9300) },
        new() { Name = "Studenica Monastery", Category = "Monastery", Region = "Raška",
                Description = "12th-century UNESCO World Heritage monastery with Byzantine frescoes.",
                Location = At(43.4864, 20.5317) },
        new() { Name = "Niš Fortress", Category = "Fortress", Region = "Nišava",
                Description = "Ottoman-era fortress on the Nišava river in the centre of Niš.",
                Location = At(43.3247, 21.8958) },
        new() { Name = "Đavolja Varoš", Category = "Nature", Region = "Toplica",
                Description = "Over two hundred natural earth pyramids formed by erosion.",
                Location = At(42.9936, 21.4011) },
    ];
}
