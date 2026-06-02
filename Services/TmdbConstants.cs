namespace BlazorWebAppMovies.Services;

public static class TmdbConstants
{
    private const string ImageBase = "https://image.tmdb.org/t/p/";

    public static string? PosterUrl(string? path)       => path is null ? null : $"{ImageBase}w500{path}";
    public static string? BackdropUrl(string? path)     => path is null ? null : $"{ImageBase}w1280{path}";
    public static string? ThumbUrl(string? path)        => path is null ? null : $"{ImageBase}w154{path}";
    public static string? PersonPhotoUrl(string? path)  => path is null ? null : $"{ImageBase}w185{path}";
    public static string? ProviderLogoUrl(string? path) => path is null ? null : $"{ImageBase}w45{path}";
    public static string? SearchPosterUrl(string? path) => path is null ? null : $"{ImageBase}w200{path}";

    public const string DirectorJob = "Director";
    public const string YouTubeSite = "YouTube";
    public const string TrailerType = "Trailer";

    public const int MaxCastMembers     = 10;
    public const int MaxRecommendations = 12;
    public const int MaxActingCredits   = 24;
    public const int MinVoteCount       = 10;
    public const int MaxBioLength       = 500;
    public const int ApiDelayMs         = 300;
}
