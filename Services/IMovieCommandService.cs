using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface IMovieCommandService
{
    Task ToggleFavoriteAsync(int id);
    Task SaveCastAsync(int movieId, List<CastMember> cast);
    Task BackfillPersonIdsAsync(int movieId, int? directorTmdbId, List<CastMember> cast);
    Task AddAsync(Movie movie);
    Task AddIfNotExistsAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(int id);
}
