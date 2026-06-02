using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public interface ITmdbSearchService
{
    Task<List<TmdbMovieResult>> SearchAsync(string query);
    Task<List<int>> DiscoverClassicIdsAsync();
    Task<TmdbMovieResult?> FindByImdbIdAsync(string imdbId);
    Task<TmdbMovieResult?> SearchFirstAsync(string title, int? year);
}
