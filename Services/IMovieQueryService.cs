using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface IMovieQueryService
{
    Task<(List<Movie> Movies, int TotalCount)> GetPagedAsync(string? search = null, string? genre = null, string sortBy = "title", bool ascending = true, int page = 1, int pageSize = 25, bool favoritesOnly = false);
    Task<List<string>> GetGenresAsync();
    Task<int?> GetRandomIdAsync();
    Task<(int TotalMovies, int FavoriteMovies)> GetStatsAsync();
    Task<Movie?> GetByIdAsync(int id);
    Task<Dictionary<int, int>> GetLocalIdsByTmdbIdsAsync(IEnumerable<int> tmdbIds);
}
