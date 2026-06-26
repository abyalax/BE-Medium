# .NET & EF Core Command Documentation

## Prerequisites

Before getting started, make sure the following tools are installed on your machine:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or a newer version.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or use docker on wsl.

## Entity Framework Core (Database Schema Management)

### Migrate UP (Apply Schema Changes)

Used to create a new database or update the current database schema to the latest migration version.

```bash
# Apply all pending migrations
dotnet ef database update

# Apply migrations only up to a specific migration file
dotnet ef database update [SpecificMigrationName]
```

### Migrate DOWN (Rollback / Revert Migrations)

In EF Core, rolling back a migration is done by updating the database to a previous migration target.

```bash
# Roll back to a specific migration (removes objects created after it)
dotnet ef database update [PreviousMigrationName]

# Full reset: roll back all migrations and return the database to an empty state
dotnet ef database update 0
```

### Add & Remove Migration Files (C# Code Generation)

```bash
# Generate a new migration file after modifying C# models/configuration
dotnet ef migrations add [NewMigrationName]

# Remove the most recent migration file that has not yet been applied to the database
dotnet ef migrations remove
```

---

## Data Seeding Commands (Custom Script)

Use these commands to execute the deterministic mock data generator that has been separated into a dedicated script runner.

```bash
# Run manual data seeding (automatically applies pending migrations first)
dotnet run -- --seed

# Combined operation: Drop the physical database + Migrate Up + Seed fresh data in a single command
dotnet ef database drop --force && dotnet run -- --seed
```

---

## Development Environment Commands

### Dev Watch (Hot Reload)

Monitors C# code changes and reloads the application automatically without requiring a manual restart. This is especially useful during API development.

```bash
# Run the application with automatic hot reload
dotnet watch run

# Run dev watch using a specific launchSettings profile (e.g., https/http)
dotnet watch run --launch-profile "https"
```

---

## Build, Compile, & Maintenance Commands

### Build & Compile

```bash
# Compile the application to check for syntax and compiler errors
dotnet build

# Perform a clean build by ignoring previous build caches
dotnet build --no-incremental
```

### Run (Normal Mode)

```bash
# Run the web application normally (without seeding and without watch mode)
dotnet run
```

### Clean & Restore Dependencies

```bash
# Restore missing or not-yet-installed NuGet packages
dotnet restore

# Remove generated bin/ and obj/ directories from previous builds
dotnet clean
```

### Format Code

```bash
# Format all files with config .editorconfig
dotnet format
```

## API Testing with Bruno

The project includes a `bruno/` directory containing all API collections required for testing and development.

### Import Bruno Collections

1. Open Bruno.
2. Select **Open Collection** or **Import Collection**.
3. Navigate to the project's `bruno/` folder.
4. Import the collection directly from that directory.

No additional setup is required. The collection is maintained within the repository and should stay synchronized with the available API endpoints.

_Last Updated 24 Juny 2026_
