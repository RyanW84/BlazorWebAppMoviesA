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
        var ids = new HashSet<int>();

        // 5-year windows from 1970–2024, 10 pages each → ~2,200 unique IDs
        (string Start, string End)[] windows =
        [
            ("1970-01-01", "1974-12-31"),
            ("1975-01-01", "1979-12-31"),
            ("1980-01-01", "1984-12-31"),
            ("1985-01-01", "1989-12-31"),
            ("1990-01-01", "1994-12-31"),
            ("1995-01-01", "1999-12-31"),
            ("2000-01-01", "2004-12-31"),
            ("2005-01-01", "2009-12-31"),
            ("2010-01-01", "2014-12-31"),
            ("2015-01-01", "2019-12-31"),
            ("2020-01-01", "2024-12-31"),
        ];

        foreach (var (start, end) in windows)
        {
            for (var page = 1; page <= 10; page++)
            {
                try
                {
                    var response = await httpClient.GetAsync(
                        $"discover/movie?api_key={_apiKey}" +
                        $"&primary_release_date.gte={start}&primary_release_date.lte={end}" +
                        $"&sort_by=vote_count.desc&vote_count.gte=100&page={page}&language=en-US");

                    if (!response.IsSuccessStatusCode) break;

                    var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
                    foreach (var r in result?.Results ?? [])
                        ids.Add(r.Id);
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "TMDB discover failed on page {Page} for {Start}–{End}", page, start, end);
                    break;
                }
            }
        }

        return [.. ids];
    }

    public async Task<string?> GetTrailerKeyAsync(int tmdbId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}/videos?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
            var trailer = data?.Results
                .Where(v => v.Site == "YouTube" && v.Type == "Trailer")
                .OrderByDescending(v => v.Official)
                .FirstOrDefault();
            return trailer?.Key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB videos fetch failed for tmdbId={TmdbId}", tmdbId);
            return null;
        }
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
