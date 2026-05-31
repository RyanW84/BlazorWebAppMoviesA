using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public interface ITmdbPersonService
{
    Task<TmdbPersonDetails?> GetPersonDetailsAsync(int personId);
    Task<TmdbPersonMovieCreditsResponse?> GetPersonMovieCreditsAsync(int personId);
}
