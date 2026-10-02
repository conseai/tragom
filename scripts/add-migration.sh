#!/usr/bin/env sh
# Creates a new EF Core migration inside the API container (no local .NET SDK needed).
# Usage: ./scripts/add-migration.sh AddReviewsTable
set -e
NAME="$1"
if [ -z "$NAME" ]; then
  echo "Usage: ./scripts/add-migration.sh <MigrationName>   (for example: AddReviewsTable)"
  exit 1
fi
cd "$(dirname "$0")/.."

docker compose run --rm --no-deps --entrypoint sh api -c \
  "dotnet tool restore && dotnet ef migrations add '$NAME' --project TravelApp.Api --output-dir Data/Migrations"

echo ""
echo "Migration '$NAME' created in backend/TravelApp.Api/Data/Migrations."
echo "Apply it by restarting the API:  docker compose restart api"
echo "Commit the migration files together with the model change."
