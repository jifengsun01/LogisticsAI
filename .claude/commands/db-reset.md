Reset the local development database from scratch.

Steps:
1. Stop and remove the existing logistics-postgres Docker container (if it exists)
2. Recreate it fresh using postgres:16 with POSTGRES_USER=postgres, POSTGRES_PASSWORD=postgres, POSTGRES_DB=logistics_ai, on port 5432
3. Wait a moment for Postgres to be ready
4. Run `dotnet ef database update` from the LogisticsAI.Api directory to apply all migrations
5. Tell the user to run `dotnet run` in LogisticsAI.Api to seed the data (seeding happens on app startup, not during migration)
