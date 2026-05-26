# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (HTTP)
dotnet run --launch-profile http

# Run the app (HTTPS)
dotnet run --launch-profile https

# Build
dotnet build

# Add an EF Core migration
dotnet ef migrations add <MigrationName>

# Apply migrations to the database
dotnet ef database update

# Scaffold CRUD pages for a model
dotnet aspnet-codegenerator razorpage -m <ModelName> -dc <DbContextName> --relativeFolderPath Components/Pages/<ModelName> --namespaceName BlazorWebAppMovies.Components.Pages.<ModelName> -sqlite
```

Dev server runs at `http://localhost:5176` (HTTP) or `https://localhost:7212` (HTTPS).

## TMDB API Key

Add your TMDB API key to `appsettings.Development.json` under `Tmdb:ApiKey`. Get a free key at [themoviedb.org](https://www.themoviedb.org/settings/api). The `appsettings.json` connection string (`Data Source=movies.db`) creates a local SQLite file at the project root on first run — no migration needed, `EnsureCreated()` handles it.

## Architecture

This is a **Blazor Web App** targeting .NET 10 with **Interactive Server** render mode. EF Core uses SQLite (connection string in `appsettings.json`).

**Render pipeline:** `App.razor` is the root HTML document → `<Routes />` → `Router` matches URLs to pages under `Components/Pages/`. All pages default to `MainLayout`.

**Interactivity model:** Static SSR is the default. Pages that need user interaction (search, forms, button clicks) opt in with `@rendermode InteractiveServer`. Read-only pages like `Details.razor` stay as static SSR.

**Service layer:**

- `IMovieService` / `MovieService` — all local DB operations (search, sort, CRUD). Registered as scoped.
- `ITmdbService` / `TmdbService` — TMDB API v3 client. Searches movies and imports full details (title, year, genre, director, rating, synopsis, poster URL) via `append_to_response=credits`. Registered as a typed `HttpClient`.

**Data flow for movie import:** `Create.razor` calls `ITmdbService.SearchAsync()` to show TMDB results, then `ImportAsync(tmdbId)` fetches full details + credits in one request and maps them into a `Movie` entity. Properties are copied onto the existing form model (not replacing the reference) to keep `EditForm`'s `EditContext` valid.

**Search and sort:** `MovieService.GetAllAsync()` accepts optional `search` (filters title and director), `sortBy` (`"title"` / `"year"` / `"rating"`), and `ascending` — all applied as EF Core LINQ before hitting the database.

**Key directories:**

- `Components/Pages/Movies/` — five pages: `Index` (list + search + sort), `Details`, `Create` (with TMDB search), `Edit`, `Delete`
- `Models/Movie/` — `Movie.cs` entity with data annotations
- `Models/Tmdb/` — DTOs for TMDB API responses (`TmdbSearchResponse`, `TmdbMovieDetails`, etc.)
- `Data/` — `MovieDbContext`
- `Services/` — service interfaces and implementations

**Static assets:** Managed via `MapStaticAssets()`, referenced with `@Assets["..."]` for fingerprinted URLs. CSS isolation via `.razor.css` files scoped per component.

**Error handling:** Non-development environments use `/Error` for exceptions and `/not-found` for 404s via `UseStatusCodePagesWithReExecute`.

## Codacy Integration

A Codacy MCP server integration is configured in [.github/instructions/codacy.instructions.md](.github/instructions/codacy.instructions.md). After editing any file, run `codacy_cli_analyze` via the Codacy MCP tool for that file. After any package manager operation, run it with `tool: "trivy"` for security scanning.
