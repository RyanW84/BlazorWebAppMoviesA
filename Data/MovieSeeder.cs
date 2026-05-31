using BlazorWebAppMovies.Services;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Data;

public static class MovieSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope  = services.CreateScope();
        var db           = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        var tmdbSearch   = scope.ServiceProvider.GetRequiredService<ITmdbSearchService>();
        var tmdbImport   = scope.ServiceProvider.GetRequiredService<ITmdbImportService>();
        var logger       = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MovieSeeder");

        var existingTmdbIds = (await db.Movies
            .Where(m => m.TmdbId != null)
            .Select(m => m.TmdbId!.Value)
            .ToListAsync())
            .ToHashSet();

        if (existingTmdbIds.Count >= 2000)
        {
            logger.LogInformation("Seed: {Count} TMDB films already present, skipping", existingTmdbIds.Count);
            return;
        }

        logger.LogInformation("Seed: discovering films from TMDB (1970–2024)…");
        var ids    = await tmdbSearch.DiscoverClassicIdsAsync();
        var toSeed = ids.Where(id => !existingTmdbIds.Contains(id)).ToList();
        logger.LogInformation("Seed: {Total} discovered, {Count} to insert", ids.Count, toSeed.Count);

        var seeded = 0;
        foreach (var id in toSeed)
        {
            var movie = await tmdbImport.ImportAsync(id);
            if (movie is not null)
            {
                db.Movies.Add(movie);
                await db.SaveChangesAsync();
                seeded++;
                logger.LogInformation(
                    "Seed [{Current}/{Total}]: \"{Title}\" ({Year})",
                    seeded, toSeed.Count, movie.Title, movie.ReleaseYear);
            }
            await Task.Delay(TmdbConstants.ApiDelayMs);
        }

        logger.LogInformation("Seed complete — {Count} films added", seeded);
    }
}
