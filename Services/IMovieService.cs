using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetAllAsync(string? search = null, string sortBy = "title", bool ascending = true);
        Task<(List<Movie> Movies, int TotalCount)> GetPagedAsync(string? search = null, string? genre = null, string sortBy = "title", bool ascending = true, int page = 1, int pageSize = 25);
        Task<List<string>> GetGenresAsync();
        Task<int?> GetRandomIdAsync();
        Task SaveCastAsync(int movieId, List<CastMember> cast);
        Task<Movie?> GetByIdAsync(int id);
        Task AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(int id);
    }
}
