using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetAllAsync(string? search = null, string sortBy = "title", bool ascending = true);
        Task<Movie?> GetByIdAsync(int id);
        Task AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(int id);
    }
}
