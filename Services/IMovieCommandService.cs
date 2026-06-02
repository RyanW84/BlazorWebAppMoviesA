using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface IMovieCommandService
{
    Task ToggleFavoriteAsync(int id);
    Task SetWatchStatusAsync(int id, WatchStatus status);
    Task SetPersonalRatingAsync(int id, int? rating);
    Task SaveCastAsync(int movieId, List<CastMember> cast);
    Task BackfillPersonIdsAsync(int movieId, int? directorTmdbId, List<CastMember> cast);
    Task SetWatchDateAsync(int id, DateTime? date);
    Task AddTagAsync(int movieId, string tag);
    Task RemoveTagAsync(int movieId, string tag);
    Task AddWatchLogEntryAsync(WatchLogEntry entry);
    Task DeleteWatchLogEntryAsync(int entryId);
    Task SetNotesAsync(int id, string? notes);
    Task SetWatchlistPriorityAsync(int id, int? priority);
    Task AddAsync(Movie movie);
    Task AddIfNotExistsAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(int id);
}
