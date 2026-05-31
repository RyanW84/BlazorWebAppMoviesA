using BlazorWebAppMovies.Models.Movie;
using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public interface ITmdbService
{
    Task<List<TmdbMovieResult>> SearchAsync(string query);
    Task<Movie?> ImportAsync(int tmdbId);
    Task<TmdbPersonDetails?> GetPersonDetailsAsync(int personId);
    Task<TmdbPersonMovieCreditsResponse?> GetPersonMovieCreditsAsync(int personId);
    Task<List<int>> DiscoverClassicIdsAsync();
    Task<string?> GetTrailerKeyAsync(int tmdbId);
    Task<List<CastMember>> FetchCastAsync(int tmdbId);
    Task<(List<CastMember> Cast, int? DirectorTmdbId)> FetchCreditsAsync(int tmdbId);
    Task<TmdbWatchRegion?> GetWatchProvidersAsync(int tmdbId, string region = "US");
    Task<List<TmdbMovieResult>> GetRecommendationsAsync(int tmdbId);
}
