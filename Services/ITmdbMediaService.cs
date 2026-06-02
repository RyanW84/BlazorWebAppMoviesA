using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public interface ITmdbMediaService
{
    Task<string?> GetTrailerKeyAsync(int tmdbId);
    Task<List<TmdbMovieResult>> GetRecommendationsAsync(int tmdbId);
    Task<TmdbWatchRegion?> GetWatchProvidersAsync(int tmdbId, string region = "US");
    Task<TmdbCollectionDetails?> GetCollectionAsync(int collectionId);
    Task<List<TmdbMovieResult>> GetTrendingAsync(string window = "week");
}
