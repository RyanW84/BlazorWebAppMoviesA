using BlazorWebAppMovies.Models.Movie;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Data
{
    public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
    {
        public DbSet<Movie> Movies { get; set; }
    }
}
