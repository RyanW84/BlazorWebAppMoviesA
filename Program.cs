using BlazorWebAppMovies.Components;
using BlazorWebAppMovies.Data;
using BlazorWebAppMovies.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MovieDb")));

builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<IMovieService>(sp      => sp.GetRequiredService<MovieService>());
builder.Services.AddScoped<IMovieQueryService>(sp  => sp.GetRequiredService<MovieService>());
builder.Services.AddScoped<IMovieCommandService>(sp => sp.GetRequiredService<MovieService>());

builder.Services.AddScoped<IMovieImportOrchestrator, MovieImportOrchestrator>();
builder.Services.AddHostedService<CastBackfillService>();

builder.Services.AddHttpClient<TmdbService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<ITmdbService>(sp        => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbSearchService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbImportService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbCreditService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbMediaService>(sp   => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbPersonService>(sp  => sp.GetRequiredService<TmdbService>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    db.Database.EnsureCreated();
}

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode(); //

app.MapGet("/export/movies.csv", async (MovieDbContext db) =>
{
    var movies = await db.Movies.AsNoTracking().OrderBy(m => m.Title).ToListAsync();
    var csv = new System.Text.StringBuilder();
    csv.AppendLine("Id,Title,Year,Genre,Director,Rating,PersonalRating,WatchStatus,Runtime,Synopsis,TmdbId,PosterUrl");
    foreach (var m in movies)
    {
        csv.AppendLine(string.Join(",",
            m.Id,
            CsvEscape(m.Title),
            m.ReleaseYear,
            CsvEscape(m.Genre),
            CsvEscape(m.Director),
            m.Rating,
            m.PersonalRating?.ToString() ?? "",
            m.WatchStatus,
            m.RuntimeMinutes?.ToString() ?? "",
            CsvEscape(m.Synopsis),
            m.TmdbId?.ToString() ?? "",
            CsvEscape(m.PosterUrl)));
    }
    return Results.File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
        "text/csv", "movies.csv");

    static string CsvEscape(string? v)
    {
        if (v is null) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }
});

app.MapGet("/export/letterboxd.csv", async (MovieDbContext db) =>
{
    var movies = await db.Movies.AsNoTracking().OrderBy(m => m.Title).ToListAsync();
    var tags   = await db.MovieTags.AsNoTracking().ToListAsync();
    var tagMap = tags.GroupBy(t => t.MovieId).ToDictionary(g => g.Key, g => g.Select(t => t.Tag).ToList());
    var log    = await db.WatchLog.AsNoTracking().OrderByDescending(w => w.WatchedOn).ToListAsync();
    var logMap = log.GroupBy(w => w.MovieId).ToDictionary(g => g.Key, g => g.ToList());

    var csv = new System.Text.StringBuilder();
    csv.AppendLine("Date,Name,Year,Letterboxd URI,Rating,Rewatch,Tags,Watched Date,Notes");
    foreach (var m in movies)
    {
        var watchedDate = logMap.TryGetValue(m.Id, out var entries) ? entries.First().WatchedOn : m.DateWatched;
        var date        = watchedDate?.ToString("yyyy-MM-dd") ?? "";
        var rating      = m.PersonalRating.HasValue ? m.PersonalRating.Value.ToString() : "";
        var rewatch     = logMap.TryGetValue(m.Id, out var e2) && e2.Count > 1 ? "true" : "";
        var tagList     = tagMap.TryGetValue(m.Id, out var t) ? string.Join("|", t) : "";
        csv.AppendLine(string.Join(",",
            date,
            LbEscape(m.Title),
            m.ReleaseYear,
            "",
            rating,
            rewatch,
            LbEscape(tagList),
            date,
            LbEscape(m.Notes)));
    }
    return Results.File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "letterboxd.csv");

    static string LbEscape(string? v)
    {
        if (v is null) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }
});

app.MapGet("/export/movies.json", async (MovieDbContext db) =>
{
    var movies = await db.Movies.AsNoTracking()
        .Include(m => m.Cast)
        .OrderBy(m => m.Title)
        .ToListAsync();

    var tags = await db.MovieTags.AsNoTracking().ToListAsync();
    var tagMap = tags.GroupBy(t => t.MovieId)
        .ToDictionary(g => g.Key, g => g.Select(t => t.Tag).ToList());

    var log = await db.WatchLog.AsNoTracking().OrderBy(w => w.WatchedOn).ToListAsync();
    var logMap = log.GroupBy(w => w.MovieId)
        .ToDictionary(g => g.Key, g => g.Select(w => new { date = w.WatchedOn.ToString("yyyy-MM-dd"), w.Notes }).ToList());

    var export = movies.Select(m => new
    {
        m.Id, m.Title, m.ReleaseYear, m.Genre, m.Director, m.Rating,
        m.PersonalRating, WatchStatus = m.WatchStatus.ToString(),
        m.RuntimeMinutes, m.Synopsis, m.PosterUrl, m.TmdbId,
        m.IsFavorite, m.CollectionName, m.CollectionId,
        DateWatched = m.DateWatched?.ToString("yyyy-MM-dd"),
        m.Notes,
        Tags = tagMap.TryGetValue(m.Id, out var t) ? t : [],
        WatchLog = logMap.TryGetValue(m.Id, out var l) ? l : [],
        Cast = m.Cast.OrderBy(c => c.Order).Select(c => new { c.Name, c.Character, c.Order }).ToList(),
    });

    var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    return Results.File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "movies.json");
});

app.Lifetime.ApplicationStarted.Register(() =>
    _ = Task.Run(() => MovieSeeder.SeedAsync(app.Services)));

app.Run();
//
//