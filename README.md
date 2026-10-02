# Travel App

Location-based travel platform. Angular frontend, .NET 10 API, PostgreSQL 17 with PostGIS.

## Quick start

Requirements: Docker Desktop (with WSL2 on Windows) and Git.

```bash
cp .env.example .env
docker compose up --build
```

Then open:

- Web app: http://localhost:4200
- API health check: http://localhost:8080/api/health
- OpenAPI description: http://localhost:8080/openapi/v1.json

**New to the project? Read [docs/TUTORIAL.md](docs/TUTORIAL.md).** It covers installation, the daily workflow, database migrations and troubleshooting.

## Stack

| Layer | Technology | Location |
|---|---|---|
| Frontend | Angular 22, TypeScript, SCSS | `frontend/` |
| API | ASP.NET Core 10 minimal APIs, EF Core 10 | `backend/TravelApp.Api/` |
| Spatial | NetTopologySuite + Npgsql, PostGIS `geography(Point, 4326)` | `Domain/Place.cs`, `Endpoints/PlacesEndpoints.cs` |
| Database | PostgreSQL 17 + PostGIS 3.5 | `docker-compose.yml` |
| CI | GitHub Actions: build on every PR, publish images to GHCR on `main` | `.github/workflows/ci.yml` |

## Common commands

| Task | Command |
|---|---|
| Start / stop | `docker compose up -d` / `docker compose down` |
| API logs | `docker compose logs -f api` |
| New database migration | `./scripts/add-migration.sh <Name>` then `docker compose restart api` |
| Database shell | `./scripts/psql.sh` |
| Reset your local database | `./scripts/reset-db.sh` |
