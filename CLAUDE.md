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

# Add a EF Core migration
dotnet ef migrations add <MigrationName>

# Apply migrations to the database
dotnet ef database update

# Scaffold CRUD pages for a model
dotnet aspnet-codegenerator razorpage -m <ModelName> -dc <DbContextName> --relativeFolderPath Components/Pages/<ModelName> --namespaceName BlazorWebAppMovies.Components.Pages.<ModelName> -sqlite
```

Dev server runs at `http://localhost:5176` (HTTP) or `https://localhost:7212` (HTTPS).

## Architecture

This is a **Blazor Web App** targeting .NET 10 with **Interactive Server** render mode. The app uses EF Core with both SQLite (dev) and SQL Server (prod) providers already referenced.

**Render pipeline:** `App.razor` is the root HTML document. It renders `<Routes />`, which uses `Router` to match URLs to page components under `Components/Pages/`. All pages default to `MainLayout`.

**Interactivity model:** `AddInteractiveServerComponents()` + `AddInteractiveServerRenderMode()` — components that need interactivity must opt in with `@rendermode InteractiveServer`. Static SSR is the default.

**Key directories:**
- `Components/Pages/` — routable page components (`@page` directive)
- `Components/Layout/` — `MainLayout`, `NavMenu`, and `ReconnectModal` (the reconnect modal has its own JS in `ReconnectModal.razor.js`)
- `Models/` — EF Core entity models (empty; `Movie/` subdirectory is the intended location for the Movie model)
- `Data/` — intended location for the `DbContext`

**Static assets:** Managed via `MapStaticAssets()` and referenced with `@Assets["..."]` for fingerprinted URLs. CSS isolation is used (`.razor.css` files scoped per component).

**Error handling:** Non-development environments use `/Error` for exceptions and `/not-found` for 404s via `UseStatusCodePagesWithReExecute`.

## Codacy Integration

A Codacy MCP server integration is configured in [.github/instructions/codacy.instructions.md](.github/instructions/codacy.instructions.md). After editing any file, run `codacy_cli_analyze` via the Codacy MCP tool for that file. After any package manager operation, run it with `tool: "trivy"` for security scanning.
