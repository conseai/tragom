#!/usr/bin/env sh
# Deletes your LOCAL database and recreates it from migrations + seed data.
# Only affects your machine; nobody else's data is touched.
set -e
cd "$(dirname "$0")/.."
printf "This deletes all data in your local database. Continue? [y/N] "
read -r answer
[ "$answer" = "y" ] || [ "$answer" = "Y" ] || { echo "Cancelled."; exit 0; }

docker compose stop api db
docker compose rm -f db
docker volume rm travel-app_pgdata || true
docker compose up -d db api
echo "Database recreated. Follow progress with: docker compose logs -f api"
