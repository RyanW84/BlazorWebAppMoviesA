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
        [JsonPropertyName("crew")]
        public List<TmdbCrewMember> Crew { get; set; } = [];
    }

    public class TmdbCrewMember
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("job")]
        public string Job { get; set; } = "";
    }
}
