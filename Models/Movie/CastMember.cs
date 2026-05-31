using System.ComponentModel.DataAnnotations;

namespace BlazorWebAppMovies.Models.Movie
{
    public class CastMember
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(200)]
        public string? Character { get; set; }

        public int Order { get; set; }

        public string? ProfileUrl { get; set; }

        public int? TmdbPersonId { get; set; }
    }
}
