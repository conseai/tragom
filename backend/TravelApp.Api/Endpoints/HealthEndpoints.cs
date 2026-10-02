using Microsoft.EntityFrameworkCore;
using TravelApp.Api.Data;

namespace TravelApp.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/health: confirms the API is up and can talk to PostgreSQL + PostGIS
        app.MapGet("/api/health", async (AppDbContext db, ILogger<AppDbContext> log) =>
        {
            try
            {
                var postgis = await db.Database
                    .SqlQueryRaw<string>("SELECT postgis_lib_version() AS \"Value\"")
                    .FirstAsync();
                return Results.Ok(new { api = "ok", database = "ok", postgis });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Health check could not query the database");
                return Results.Json(
                    new { api = "ok", database = "unreachable", postgis = (string?)null },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).WithTags("Health");

        return app;
    }
}
