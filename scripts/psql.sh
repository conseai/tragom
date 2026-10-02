#!/usr/bin/env sh
# Opens a psql shell inside the database container. Type \q to exit.
cd "$(dirname "$0")/.."
docker compose exec db sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
