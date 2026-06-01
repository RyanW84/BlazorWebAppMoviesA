using System.ComponentModel.DataAnnotations;

namespace BlazorWebAppMovies.Models.Movie
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Range(1888, 2100)]
        public int ReleaseYear { get; set; }

        [StringLength(100)]
        public string? Genre { get; set; }

        [StringLength(100)]
        public string? Director { get; set; }

        [Range(0, 10)]
        public decimal Rating { get; set; }

        public string? Synopsis { get; set; }

        public string? PosterUrl { get; set; }

        public int? TmdbId { get; set; }

        public int? DirectorTmdbId { get; set; }

        public bool IsFavorite { get; set; }

        public WatchStatus WatchStatus { get; set; } = WatchStatus.None;

        [Range(1, 5)]
        public int? PersonalRating { get; set; }

        public int? RuntimeMinutes { get; set; }

        public int? CollectionId { get; set; }

        [StringLength(200)]
        public string? CollectionName { get; set; }

        public bool CastBackfilled { get; set; }

        public DateTime? DateWatched { get; set; }

        public string? Notes { get; set; }

        public int? WatchlistPriority { get; set; }

        public List<CastMember> Cast { get; set; } = [];
    }
}
