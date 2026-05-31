using BlazorWebAppMovies.Models.Movie;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Data
{
    public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(e =>
            {
                e.HasIndex(m => m.TmdbId).IsUnique();
                e.HasIndex(m => m.IsFavorite);
                e.HasIndex(m => m.Director);
                e.HasIndex(m => m.Title);
            });

            modelBuilder.Entity<CastMember>(e =>
            {
                e.HasIndex(c => c.TmdbPersonId);
                e.HasIndex(c => c.Name);
            });
        }
    }
}
