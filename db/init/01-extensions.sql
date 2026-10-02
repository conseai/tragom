-- Runs once, the first time the database volume is created.
-- EF Core migrations also ensure PostGIS exists; this makes it available even before the first migration.
CREATE EXTENSION IF NOT EXISTS postgis;
