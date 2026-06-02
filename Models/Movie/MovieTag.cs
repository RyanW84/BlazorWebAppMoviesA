using System.ComponentModel.DataAnnotations;

namespace BlazorWebAppMovies.Models.Movie;

public class MovieTag
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    [Required]
    [StringLength(50)]
    public string Tag { get; set; } = "";
}
