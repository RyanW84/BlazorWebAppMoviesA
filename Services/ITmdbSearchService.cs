using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public interface ITmdbSearchService
{
    Task<List<TmdbMovieResult>> SearchAsync(string query);
    Task<List<int>> DiscoverClassicIdsAsync();
}
