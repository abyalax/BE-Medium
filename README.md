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

## AI Summarization Model Setup

This project utilizes the **Phi-3-mini-4k-instruct** ONNX model. Due to its large size, the model files are excluded from Git and must be downloaded manually.

### Download Model
Download all files from the official Hugging Face repository:
👉 [Microsoft Phi-3-mini-4k-instruct-onnx (CUDA INT4)](https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/tree/main/cuda/cuda-int4-rtn-block-32)

### Directory Structure
Place the downloaded files into the `Assets/Model` directory exactly as shown below:

```
└── Assets/
    └── Model/
        ├── added_tokens.json                                        306 Bytes
        ├── config.json                                              919 Bytes
        ├── configuration_phi3.py                                    10.4 kB
        ├── genai_config.json                                        1.74 kB
        ├── phi3-mini-4k-instruct-cuda-int4-rtn-block-32.onnx        223 kB
        ├── phi3-mini-4k-instruct-cuda-int4-rtn-block-32.onnx.data   2.29 GB
        ├── special_tokens_map.json                                  599 Bytes
        ├── tokenizer_config.json                                    1.94 MB
        ├── tokenizer.json                                           500 kB
        └── tokenizer.model                                          3.44 kB
```
> ⚠️ **Note:** Ensure `Assets/Model/` is added to your `.gitignore` to prevent pushing large binary files to GitHub.

### Download CUDA 12.8 Toolkit
https://developer.nvidia.com/cuda-12-8-0-download-archive?target_os=Windows&target_arch=x86_64&target_version=11&target_type=exe_local

### Download cuDNN 9.23.2
https://developer.nvidia.com/cudnn-downloads

## API Testing with Bruno

The project includes a `bruno/` directory containing all API collections required for testing and development.

### Import Bruno Collections

1. Open Bruno.
2. Select **Open Collection** or **Import Collection**.
3. Navigate to the project's `bruno/` folder.
4. Import the colln directly from that directory.

No additional setup is required. The collection is maintained within the repository and should stay synchronized with the available API endpoints.

_Last Updated 30 Juny 2026_
