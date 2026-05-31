using System.Text.Json.Serialization;

namespace BlazorWebAppMovies.Models.Tmdb
{
    public class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMovieResult> Results { get; set; } = [];
    }

    public class TmdbMovieResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = "";

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }

    public class TmdbMovieDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = "";

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = [];

        [JsonPropertyName("credits")]
        public TmdbCredits Credits { get; set; } = new();
    }

    public class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    public class TmdbCredits
    {
        [JsonPropertyName("cast")]
        public List<TmdbCastMember> Cast { get; set; } = [];

        [JsonPropertyName("crew")]
        public List<TmdbCrewMember> Crew { get; set; } = [];
    }

    public class TmdbCastMember
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("character")]
        public string? Character { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }
    }

    public class TmdbCrewMember
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("job")]
        public string Job { get; set; } = "";
    }

    public class TmdbPersonDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }

        [JsonPropertyName("biography")]
        public string? Biography { get; set; }

        [JsonPropertyName("known_for_department")]
        public string KnownForDepartment { get; set; } = "";
    }

    public class TmdbPersonMovieCreditsResponse
    {
        [JsonPropertyName("cast")]
        public List<TmdbPersonCastCredit> Cast { get; set; } = [];

        [JsonPropertyName("crew")]
        public List<TmdbPersonCrewCredit> Crew { get; set; } = [];
    }

    public class TmdbPersonCastCredit
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("character")]
        public string? Character { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }
    }

    public class TmdbPersonCrewCredit
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("job")]
        public string Job { get; set; } = "";

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }
    }

    public class TmdbVideosResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbVideoResult> Results { get; set; } = [];
    }

    public class TmdbWatchProvidersResponse
    {
        [JsonPropertyName("results")]
        public Dictionary<string, TmdbWatchRegion> Results { get; set; } = [];
    }

    public class TmdbWatchRegion
    {
        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("flatrate")]
        public List<TmdbWatchProvider> Flatrate { get; set; } = [];

        [JsonPropertyName("rent")]
        public List<TmdbWatchProvider> Rent { get; set; } = [];

        [JsonPropertyName("buy")]
        public List<TmdbWatchProvider> Buy { get; set; } = [];
    }

    public class TmdbWatchProvider
    {
        [JsonPropertyName("provider_name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("logo_path")]
        public string? LogoPath { get; set; }

        [JsonPropertyName("display_priority")]
        public int DisplayPriority { get; set; }
    }

    public class TmdbVideoResult
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("site")]
        public string Site { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("official")]
        public bool Official { get; set; }
    }
}
