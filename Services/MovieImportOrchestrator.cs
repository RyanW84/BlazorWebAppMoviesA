using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public class MovieImportOrchestrator(
    ITmdbImportService tmdb,
    IMovieQueryService query,
    IMovieCommandService commands) : IMovieImportOrchestrator
{
    public async Task<Dictionary<int, int>> EnsureLocalAsync(IEnumerable<int> tmdbIds)
    {
        var ids      = tmdbIds.ToList();
        var localIds = await query.GetLocalIdsByTmdbIdsAsync(ids);

        var toImport = ids.Where(id => !localIds.ContainsKey(id)).ToList();
        if (toImport.Count > 0)
        {
            var fetched = await Task.WhenAll(toImport.Select(id => tmdb.ImportAsync(id)));
            foreach (var movie in fetched.OfType<Movie>())
                await commands.AddIfNotExistsAsync(movie);
            localIds = await query.GetLocalIdsByTmdbIdsAsync(ids);
        }

        return localIds;
    }
}
