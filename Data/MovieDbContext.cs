using BlazorWebAppMovies.Models.Movie;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebAppMovies.Data
{
    public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<WatchLogEntry> WatchLog { get; set; }
        public DbSet<Screening> Screenings { get; set; }

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

            modelBuilder.Entity<MovieTag>(e =>
            {
                e.HasIndex(t => new { t.MovieId, t.Tag }).IsUnique();
                e.HasIndex(t => t.Tag);
            });

            modelBuilder.Entity<WatchLogEntry>(e =>
            {
                e.HasIndex(w => w.MovieId);
                e.HasIndex(w => w.WatchedOn);
            });

            modelBuilder.Entity<Screening>(e =>
            {
                e.HasIndex(s => s.ScheduledFor);
                e.HasIndex(s => s.MovieId);
            });
        }
    }
}
