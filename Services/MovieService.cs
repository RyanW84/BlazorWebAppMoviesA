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
        {
            var terms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var term in terms)
            {
                var t = term.ToLower();
                var isYear = int.TryParse(term, out var parsedYear);
                var y = parsedYear;
                query = query.Where(m =>
                    m.Title.ToLower().Contains(t) ||
                    (m.Director != null && m.Director.ToLower().Contains(t)) ||
                    (m.Genre != null && m.Genre.ToLower().Contains(t)) ||
                    (isYear && m.ReleaseYear == y));
            }
        }

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

    public async Task<(List<Movie> Movies, int TotalCount)> GetPagedAsync(string? search = null, string? genre = null, string sortBy = "title", bool ascending = true, int page = 1, int pageSize = 25)
    {
        var query = db.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var terms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var term in terms)
            {
                var t = term.ToLower();
                var isYear = int.TryParse(term, out var parsedYear);
                var y = parsedYear;
                query = query.Where(m =>
                    m.Title.ToLower().Contains(t) ||
                    (m.Director != null && m.Director.ToLower().Contains(t)) ||
                    (m.Genre != null && m.Genre.ToLower().Contains(t)) ||
                    (isYear && m.ReleaseYear == y));
            }
        }

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(genre.ToLower()));

        query = (sortBy.ToLower(), ascending) switch
        {
            ("year", true) => query.OrderBy(m => m.ReleaseYear),
            ("year", false) => query.OrderByDescending(m => m.ReleaseYear),
            ("rating", true) => query.OrderBy(m => m.Rating),
            ("rating", false) => query.OrderByDescending(m => m.Rating),
            (_, true) => query.OrderBy(m => m.Title),
            (_, false) => query.OrderByDescending(m => m.Title),
        };

        var total = await query.CountAsync();
        var movies = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (movies, total);
    }

    public async Task SaveCastAsync(int movieId, List<CastMember> cast)
    {
        var existing = db.CastMembers.Where(c => c.MovieId == movieId);
        db.CastMembers.RemoveRange(existing);
        foreach (var member in cast)
            member.MovieId = movieId;
        db.CastMembers.AddRange(cast);
        await db.SaveChangesAsync();
    }

    public async Task<int?> GetRandomIdAsync()
    {
        var ids = await db.Movies.Select(m => m.Id).ToListAsync();
        if (ids.Count == 0) return null;
        return ids[Random.Shared.Next(ids.Count)];
    }

    public async Task<List<string>> GetGenresAsync() =>
        await db.Movies
            .Where(m => m.Genre != null && m.Genre != "")
            .Select(m => m.Genre!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

    public async Task<Movie?> GetByIdAsync(int id) =>
        await db.Movies.Include(m => m.Cast.OrderBy(c => c.Order)).FirstOrDefaultAsync(m => m.Id == id);

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
