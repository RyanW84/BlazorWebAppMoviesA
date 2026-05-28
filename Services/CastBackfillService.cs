using BlazorWebAppMovies.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Services;

public class CastBackfillService(
    IServiceScopeFactory scopeFactory,
    ILogger<CastBackfillService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the app to fully start before hitting the DB
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        var tmdb = scope.ServiceProvider.GetRequiredService<ITmdbService>();
        var movies = scope.ServiceProvider.GetRequiredService<IMovieService>();

        var missing = await db.Movies
            .Where(m => m.TmdbId != null && !db.CastMembers.Any(c => c.MovieId == m.Id))
            .Select(m => new { m.Id, m.TmdbId, m.Title })
            .ToListAsync(stoppingToken);

        if (missing.Count == 0)
        {
            logger.LogInformation("Cast backfill: all movies already have cast data");
            return;
        }

        logger.LogInformation("Cast backfill: {Count} movies missing cast — starting", missing.Count);

        var done = 0;
        foreach (var movie in missing)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                var cast = await tmdb.FetchCastAsync(movie.TmdbId!.Value);
                if (cast.Count > 0)
                {
                    await movies.SaveCastAsync(movie.Id, cast);
                    done++;
                    logger.LogInformation(
                        "Cast backfill: [{Done}/{Total}] {Title}",
                        done, missing.Count, movie.Title);
                }
                else
                {
                    logger.LogWarning(
                        "Cast backfill: no cast returned for {Title} (tmdbId={TmdbId})",
                        movie.Title, movie.TmdbId);
                }

                await Task.Delay(300, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cast backfill: failed for {Title} (tmdbId={TmdbId})", movie.Title, movie.TmdbId);
            }
        }

        logger.LogInformation(
            "Cast backfill: complete — {Done}/{Total} movies updated",
            done, missing.Count);
    }
}
