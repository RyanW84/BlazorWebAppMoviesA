using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface IMovieQueryService
{
    Task<(List<Movie> Movies, int TotalCount)> GetPagedAsync(string? search = null, string? genre = null, string sortBy = "title", bool ascending = true, int page = 1, int pageSize = 25, bool favoritesOnly = false, WatchStatus? watchStatus = null, int? yearFrom = null, int? yearTo = null, decimal? ratingMin = null, int? collectionId = null);
    Task<List<string>> GetGenresAsync();
    Task<int?> GetRandomIdAsync();
    Task<(int TotalMovies, int FavoriteMovies)> GetStatsAsync();
    Task<Movie?> GetByIdAsync(int id);
    Task<Dictionary<int, int>> GetLocalIdsByTmdbIdsAsync(IEnumerable<int> tmdbIds);
    Task<(List<PersonSummary> Items, int TotalCount)> GetDirectorsAsync(char? letter = null, string? search = null, int page = 1, int pageSize = 50);
    Task<(List<PersonSummary> Items, int TotalCount)> GetCastAsync(char? letter = null, string? search = null, int page = 1, int pageSize = 50);
    Task<HashSet<char>> GetDirectorLettersAsync();
    Task<HashSet<char>> GetCastLettersAsync();
    Task<List<(string Genre, int Count)>> GetStatsByGenreAsync();
    Task<List<(int Decade, int Count)>> GetStatsByDecadeAsync();
    Task<List<(decimal Bucket, int Count)>> GetStatsByRatingBucketAsync();
    Task<List<(WatchStatus Status, int Count)>> GetStatsByWatchStatusAsync();
    Task<List<(string Director, int Count)>> GetTopDirectorsAsync(int n = 10);
    Task<List<(string Actor, int Count)>> GetTopCastAsync(int n = 10);
    Task<List<(int Stars, int Count)>> GetStatsByPersonalRatingAsync();
    Task<(int TotalMinutes, int WatchedMinutes)> GetRuntimeStatsAsync();
    Task<List<(int CollectionId, string CollectionName, int Count)>> GetCollectionsAsync();
    Task<List<Movie>> GetWatchlistAsync();
}
