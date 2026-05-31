using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface ITmdbImportService
{
    Task<Movie?> ImportAsync(int tmdbId);
}
