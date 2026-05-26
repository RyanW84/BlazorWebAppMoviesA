using BlazorWebAppMovies.Data;
using BlazorWebAppMovies.Models.Movie;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Services;

public class MovieService(MovieDbContext db) : IMovieService
{
    public async Task<List<Movie>> GetAllAsync(string? search = null, string sortBy = "title", bool ascending = true)
    {
        var query = db.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.Contains(search) || (m.Director != null && m.Director.Contains(search)));

        query = (sortBy.ToLower(), ascending) switch
        {
            ("year", true) => query.OrderBy(m => m.ReleaseYear),
            ("year", false) => query.OrderByDescending(m => m.ReleaseYear),
            ("rating", true) => query.OrderBy(m => m.Rating),
            ("rating", false) => query.OrderByDescending(m => m.Rating),
            (_, true) => query.OrderBy(m => m.Title),
            (_, false) => query.OrderByDescending(m => m.Title),
        };

        return await query.ToListAsync();
    }

    public async Task<Movie?> GetByIdAsync(int id) =>
        await db.Movies.FindAsync(id);

    public async Task AddAsync(Movie movie)
    {
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Movie movie)
    {
        db.Movies.Update(movie);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is not null)
        {
            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
        }
    }
}
