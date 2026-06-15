using System.ComponentModel.DataAnnotations;

namespace BlazorWebAppMovies.Models.Movie
{
    public class Screening
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public DateTime ScheduledFor { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
