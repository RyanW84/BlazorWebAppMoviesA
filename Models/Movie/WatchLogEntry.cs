namespace BlazorWebAppMovies.Models.Movie;

public class WatchLogEntry
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public DateTime WatchedOn { get; set; }
    public string? Notes { get; set; }
}
