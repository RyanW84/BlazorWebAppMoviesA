namespace BlazorWebAppMovies.Models.Movie;

public static class WatchStatusExtensions
{
    public static string ToLabel(this WatchStatus s) => s switch
    {
        WatchStatus.WantToWatch => "WANT TO WATCH",
        WatchStatus.Watched     => "WATCHED",
        WatchStatus.Rewatching  => "REWATCHING",
        _                       => s.ToString().ToUpper(),
    };

    public static string ToActiveClass(this WatchStatus s) => s switch
    {
        WatchStatus.WantToWatch => "btn-info",
        WatchStatus.Watched     => "btn-success",
        WatchStatus.Rewatching  => "btn-warning",
        _                       => "btn-secondary",
    };
}
