using Microsoft.EntityFrameworkCore;
using TravelApp.Api.Data;
using TravelApp.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// In Docker the connection string comes from the ConnectionStrings__Default environment
// variable (see docker-compose.yml). When you run the API from your IDE it comes from
// appsettings.Development.json and points to the database container on localhost:5432.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is missing. Check your .env file and docker-compose.yml.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // API description at /openapi/v1.json
    await DatabaseSetup.MigrateAndSeedAsync(app);
}

app.MapHealthEndpoints();
app.MapPlacesEndpoints();

app.Run();
