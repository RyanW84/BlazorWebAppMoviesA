using BlazorWebAppMovies.Models.Movie;
using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public class TmdbService(HttpClient httpClient, IConfiguration config, ILogger<TmdbService> logger) : ITmdbService
{
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";
    private readonly string _apiKey = config["Tmdb:ApiKey"] ?? "";

    public async Task<List<TmdbMovieResult>> SearchAsync(string query)
    {
        logger.LogInformation("TMDB search: \"{Query}\"", query);
        try
        {
            var response = await httpClient.GetAsync(
                $"search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language=en-US&page=1");

            logger.LogInformation("TMDB search response: {StatusCode}", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("TMDB search error: {StatusCode} — {Body}", (int)response.StatusCode, body);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
            var results = result?.Results ?? [];
            logger.LogInformation("TMDB search returned {Count} result(s) for \"{Query}\"", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB search failed for \"{Query}\"", query);
            return [];
        }
    }

    public async Task<List<int>> DiscoverClassicIdsAsync()
    {
        var ids = new List<int>();
        for (var page = 1; page <= 5; page++)
        {
            try
            {
                var response = await httpClient.GetAsync(
                    $"discover/movie?api_key={_apiKey}" +
                    $"&primary_release_date.gte=1980-01-01&primary_release_date.lte=1999-12-31" +
                    $"&sort_by=vote_count.desc&vote_count.gte=800&page={page}&language=en-US");

                if (!response.IsSuccessStatusCode) break;

                var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
                ids.AddRange(result?.Results?.Select(r => r.Id) ?? Enumerable.Empty<int>());
                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TMDB discover failed on page {Page}", page);
                break;
            }
        }
        return ids;
    }

    public async Task<Movie?> ImportAsync(int tmdbId)
    {
        logger.LogInformation("TMDB import: tmdbId={TmdbId}", tmdbId);
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}?api_key={_apiKey}&append_to_response=credits&language=en-US");

            logger.LogInformation("TMDB import response: {StatusCode}", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("TMDB import error: {StatusCode} — {Body}", (int)response.StatusCode, body);
                return null;
            }

            var details = await response.Content.ReadFromJsonAsync<TmdbMovieDetails>();
            if (details is null)
            {
                logger.LogWarning("TMDB import: deserialization returned null for tmdbId={TmdbId}", tmdbId);
                return null;
            }

            var director = details.Credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;
            var releaseYear = int.TryParse(details.ReleaseDate?.Split('-')[0], out var year) ? year : 0;

            logger.LogInformation("TMDB import succeeded: \"{Title}\" ({Year}), director={Director}", details.Title, releaseYear, director ?? "unknown");

            return new Movie
            {
                Title = details.Title,
                ReleaseYear = releaseYear,
                Genre = details.Genres.FirstOrDefault()?.Name,
                Director = director,
                Rating = (decimal)Math.Round(details.VoteAverage, 1),
                Synopsis = details.Overview,
                PosterUrl = details.PosterPath != null ? $"{ImageBaseUrl}{details.PosterPath}" : null,
                TmdbId = details.Id
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB import failed for tmdbId={TmdbId}", tmdbId);
            return null;
        }
    }
}
