namespace BlazorWebAppMovies.Services;

public interface IMovieImportOrchestrator
{
    /// <summary>
    /// Ensures all TMDB IDs are present in the local DB, importing any that are missing.
    /// Returns a map of tmdbId → local movie Id for all supplied IDs that exist locally.
    /// </summary>
    Task<Dictionary<int, int>> EnsureLocalAsync(IEnumerable<int> tmdbIds);
}
