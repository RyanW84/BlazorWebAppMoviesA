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

        public bool IsFavorite { get; set; }

        public List<CastMember> Cast { get; set; } = [];
    }
}
