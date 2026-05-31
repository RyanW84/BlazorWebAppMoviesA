using BlazorWebAppMovies.Models.Movie;

namespace BlazorWebAppMovies.Services;

public interface ITmdbCreditService
{
    Task<(List<CastMember> Cast, int? DirectorTmdbId)> FetchCreditsAsync(int tmdbId);
    Task<List<CastMember>> FetchCastAsync(int tmdbId);
}
