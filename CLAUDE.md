# CLAUDE.md

## Commands

```bash
dotnet run --launch-profile http          # http://localhost:5176
dotnet run --launch-profile https         # https://localhost:7212
dotnet build
dotnet ef migrations add <Name>
dotnet ef database update
dotnet aspnet-codegenerator razorpage -m <Model> -dc <DbContext> --relativeFolderPath Components/Pages/<Model> --namespaceName BlazorWebAppMovies.Components.Pages.<Model> -sqlite
```

## Config

- TMDB API key → `appsettings.Development.json` under `Tmdb:ApiKey`
- SQLite DB → `Data Source=movies.db` in `appsettings.json`; `EnsureCreated()` handles schema on first run

## Architecture

**.NET 10 Blazor Web App**, Interactive Server render mode, EF Core + SQLite.

**Render pipeline:** `App.razor` → `<Routes />` → pages under `Components/Pages/` (default: `MainLayout`)

**Interactivity:** Static SSR by default. Add `@rendermode InteractiveServer` only for pages requiring user interaction (search, forms, clicks).

**Services (DI):**

- `IMovieService` / `MovieService` — DB CRUD, search, sort (scoped)
- `ITmdbService` / `TmdbService` — TMDB API v3; `SearchAsync()` + `ImportAsync(tmdbId)` via `append_to_response=credits` (typed `HttpClient`)

**Import flow:** `Create.razor` → `SearchAsync()` → `ImportAsync()` → map to `Movie` entity, copy properties onto existing form model to preserve `EditContext`.

**Search/sort:** `MovieService.GetAllAsync(search, sortBy, ascending)` — all filtering done via EF Core LINQ.

**Key paths:**

| Path | Purpose |
| --- | --- |
| `Components/Pages/Movies/` | Index, Details, Create, Edit, Delete |
| `Models/Movie/Movie.cs` | Entity + data annotations |
| `Models/Tmdb/` | TMDB DTOs |
| `Data/MovieDbContext` | EF Core context |
| `Services/` | Interfaces + implementations |

**Static assets:** `MapStaticAssets()` + `@Assets["..."]` for fingerprinted URLs. CSS isolation via `.razor.css` per component.

**Errors:** `/Error` (exceptions) + `/not-found` (404) via `UseStatusCodePagesWithReExecute` in non-dev.

## Coding Standards

### SOLID

- **Single Responsibility** — one class/service per concern; pages handle UI state only, never embed DB logic inline
- **Open/Closed** — extend via new implementations behind existing interfaces, not by modifying them
- **Liskov Substitution** — interface contracts must be fully honoured by every implementation
- **Interface Segregation** — keep `IMovieService` / `ITmdbService` focused; split if unrelated operations accumulate
- **Dependency Inversion** — depend on `IMovieService` / `ITmdbService`, never on concrete classes

### DRY

Extract any logic used in more than one place into a shared service, helper, or component. No duplicated query predicates, markup blocks, or mapping code.

### OOP

Prefer composition over inheritance. Use DTOs for TMDB responses and entities for persistence — never mix the two.

### Design Patterns

- **Repository-style service layer** (`IMovieService`) — all DB access stays behind this boundary
- **Strategy** — search/sort parameters passed into `GetAllAsync`; add new sort strategies there, not in pages
- **Mapper** — TMDB → `Movie` mapping lives in `ImportAsync`, nowhere else

### No Monoliths

If a `.razor` page or service method exceeds ~80–100 lines of logic, extract components or helper methods.

### Eliminate Redundancy

Before adding code, check whether the service layer already covers the need. Remove dead code, unused parameters, and obsolete DTOs on sight.

## Codacy Integration

After editing any file, run `codacy_cli_analyze` via the Codacy MCP tool. After package manager operations, run it with `tool: "trivy"`. Full config: [.github/instructions/codacy.instructions.md](.github/instructions/codacy.instructions.md).
