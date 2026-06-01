using BlazorWebAppMovies.Data;
using BlazorWebAppMovies.Models.Movie;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Services;

public class MovieService(MovieDbContext db) : IMovieService
{
    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task ReplaceCastAsync(int movieId, List<CastMember> cast)
    {
        await db.CastMembers.Where(c => c.MovieId == movieId).ExecuteDeleteAsync();
        foreach (var member in cast)
            member.MovieId = movieId;
        db.CastMembers.AddRange(cast);
        await db.SaveChangesAsync();
    }

    // ── IMovieQueryService ───────────────────────────────────────────────────

    public async Task<(List<Movie> Movies, int TotalCount)> GetPagedAsync(
        string? search = null, string? genre = null, string sortBy = "title",
        bool ascending = true, int page = 1, int pageSize = 25, bool favoritesOnly = false,
        WatchStatus? watchStatus = null, int? yearFrom = null, int? yearTo = null, decimal? ratingMin = null)
    {
        var query = db.Movies.AsNoTracking();

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
                    m.Cast.Any(c => c.Name.ToLower().Contains(t)) ||
                    (isYear && m.ReleaseYear == y));
            }
        }

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(genre.ToLower()));

        if (favoritesOnly)
            query = query.Where(m => m.IsFavorite);

        if (watchStatus.HasValue)
            query = query.Where(m => m.WatchStatus == watchStatus.Value);

        if (yearFrom.HasValue)
            query = query.Where(m => m.ReleaseYear >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(m => m.ReleaseYear <= yearTo.Value);

        if (ratingMin.HasValue)
            query = query.Where(m => m.Rating >= ratingMin.Value);

        query = (sortBy.ToLower(), ascending) switch
        {
            ("year",   true)  => query.OrderBy(m => m.ReleaseYear),
            ("year",   false) => query.OrderByDescending(m => m.ReleaseYear),
            ("rating", true)  => query.OrderBy(m => m.Rating),
            ("rating", false) => query.OrderByDescending(m => m.Rating),
            (_,        true)  => query.OrderBy(m => m.Title),
            (_,        false) => query.OrderByDescending(m => m.Title),
        };

        var total  = await query.CountAsync();
        var movies = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (movies, total);
    }

    public async Task<List<string>> GetGenresAsync() =>
        await db.Movies
            .Where(m => m.Genre != null && m.Genre != "")
            .Select(m => m.Genre!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

    public async Task<int?> GetRandomIdAsync() =>
        await db.Movies
            .OrderBy(_ => EF.Functions.Random())
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync();

    public async Task<(int TotalMovies, int FavoriteMovies)> GetStatsAsync()
    {
        var result = await db.Movies
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Count(), Favorites = g.Sum(m => m.IsFavorite ? 1 : 0) })
            .FirstOrDefaultAsync();
        return (result?.Total ?? 0, result?.Favorites ?? 0);
    }

    public async Task<Movie?> GetByIdAsync(int id) =>
        await db.Movies.AsNoTracking()
            .Include(m => m.Cast.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Dictionary<int, int>> GetLocalIdsByTmdbIdsAsync(IEnumerable<int> tmdbIds)
    {
        var ids = tmdbIds.ToList();
        return await db.Movies
            .Where(m => m.TmdbId != null && ids.Contains(m.TmdbId.Value))
            .ToDictionaryAsync(m => m.TmdbId!.Value, m => m.Id);
    }

    public async Task<HashSet<char>> GetDirectorLettersAsync()
    {
        var firsts = await db.Movies
            .Where(m => !string.IsNullOrEmpty(m.Director))
            .Select(m => m.Director!.Substring(0, 1).ToUpper())
            .Distinct()
            .ToListAsync();

        return firsts
            .Where(s => char.IsLetter(s[0]))
            .Select(s => s[0])
            .ToHashSet();
    }

    public async Task<HashSet<char>> GetCastLettersAsync()
    {
        var firsts = await db.CastMembers
            .Select(c => c.Name.Substring(0, 1).ToUpper())
            .Distinct()
            .ToListAsync();

        return firsts
            .Where(s => char.IsLetter(s[0]))
            .Select(s => s[0])
            .ToHashSet();
    }

    public async Task<(List<PersonSummary> Items, int TotalCount)> GetDirectorsAsync(
        char? letter = null, string? search = null, int page = 1, int pageSize = 50)
    {
        var query = db.Movies.Where(m => !string.IsNullOrEmpty(m.Director));

        if (letter.HasValue)
        {
            var prefix = letter.Value.ToString().ToUpperInvariant();
            query = query.Where(m => m.Director!.ToUpper().StartsWith(prefix));
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Director!.ToLower().Contains(search.ToLower()));
        }

        var grouped = query
            .GroupBy(m => new { m.Director, m.DirectorTmdbId })
            .Select(g => new { Name = g.Key.Director!, TmdbId = g.Key.DirectorTmdbId, Count = g.Count() })
            .OrderBy(g => g.Name);

        var total = await grouped.CountAsync();
        var rows  = await grouped.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (rows.Select(r => new PersonSummary(r.Name, r.TmdbId, r.Count)).ToList(), total);
    }

    public async Task<(List<PersonSummary> Items, int TotalCount)> GetCastAsync(
        char? letter = null, string? search = null, int page = 1, int pageSize = 50)
    {
        var query = db.CastMembers.AsQueryable();

        if (letter.HasValue)
        {
            var prefix = letter.Value.ToString().ToUpperInvariant();
            query = query.Where(c => c.Name.ToUpper().StartsWith(prefix));
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }

        var grouped = query
            .GroupBy(c => new { c.Name, c.TmdbPersonId })
            .Select(g => new { Name = g.Key.Name, TmdbId = g.Key.TmdbPersonId, Count = g.Count() })
            .OrderBy(g => g.Name);

        var total = await grouped.CountAsync();
        var rows  = await grouped.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (rows.Select(r => new PersonSummary(r.Name, r.TmdbId, r.Count)).ToList(), total);
    }

    public async Task<List<(string Genre, int Count)>> GetStatsByGenreAsync() =>
        (await db.Movies
            .Where(m => m.Genre != null && m.Genre != "")
            .GroupBy(m => m.Genre!)
            .Select(g => new { Genre = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync())
        .Select(g => (g.Genre, g.Count))
        .ToList();

    public async Task<List<(int Decade, int Count)>> GetStatsByDecadeAsync() =>
        (await db.Movies
            .GroupBy(m => m.ReleaseYear / 10 * 10)
            .Select(g => new { Decade = g.Key, Count = g.Count() })
            .OrderBy(g => g.Decade)
            .ToListAsync())
        .Select(g => (g.Decade, g.Count))
        .ToList();

    public async Task<List<(decimal Bucket, int Count)>> GetStatsByRatingBucketAsync() =>
        (await db.Movies
            .Where(m => m.Rating > 0)
            .GroupBy(m => (int)m.Rating)
            .Select(g => new { Bucket = g.Key, Count = g.Count() })
            .OrderBy(g => g.Bucket)
            .ToListAsync())
        .Select(g => ((decimal)g.Bucket, g.Count))
        .ToList();

    public async Task<List<(WatchStatus Status, int Count)>> GetStatsByWatchStatusAsync() =>
        (await db.Movies
            .GroupBy(m => m.WatchStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync())
        .Select(g => (g.Status, g.Count))
        .ToList();

    public async Task<List<(string Director, int Count)>> GetTopDirectorsAsync(int n = 10) =>
        (await db.Movies
            .Where(m => m.Director != null && m.Director != "")
            .GroupBy(m => m.Director!)
            .Select(g => new { Director = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(n)
            .ToListAsync())
        .Select(g => (g.Director, g.Count))
        .ToList();

    // ── IMovieCommandService ─────────────────────────────────────────────────

    public async Task ToggleFavoriteAsync(int id) =>
        await db.Movies
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsFavorite, m => !m.IsFavorite));

    public async Task SetWatchStatusAsync(int id, WatchStatus status) =>
        await db.Movies
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.WatchStatus, status));

    public async Task SetPersonalRatingAsync(int id, int? rating) =>
        await db.Movies
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.PersonalRating, rating));

    public async Task SaveCastAsync(int movieId, List<CastMember> cast) =>
        await ReplaceCastAsync(movieId, cast);

    public async Task BackfillPersonIdsAsync(int movieId, int? directorTmdbId, List<CastMember> cast)
    {
        var movie = await db.Movies.FindAsync(movieId);
        if (movie is null) return;

        if (directorTmdbId is not null && movie.DirectorTmdbId is null)
            movie.DirectorTmdbId = directorTmdbId;

        if (cast.Count > 0)
            await ReplaceCastAsync(movieId, cast);
        else
            await db.SaveChangesAsync();
    }

    public async Task AddAsync(Movie movie)
    {
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
    }

    public async Task AddIfNotExistsAsync(Movie movie)
    {
        if (movie.TmdbId is int tmdbId && await db.Movies.AnyAsync(m => m.TmdbId == tmdbId))
            return;
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
        await db.CastMembers.Where(c => c.MovieId == id).ExecuteDeleteAsync();
        await db.Movies.Where(m => m.Id == id).ExecuteDeleteAsync();
    }
}
