using BlazorWebAppMovies.Models.Movie;
using BlazorWebAppMovies.Models.Tmdb;

namespace BlazorWebAppMovies.Services;

public class TmdbService(HttpClient httpClient, IConfiguration config, ILogger<TmdbService> logger) : ITmdbService
{
    private readonly string _apiKey = config["Tmdb:ApiKey"] ?? "";

    // ── Private helpers ──────────────────────────────────────────────────────

    private static List<CastMember> MapCastMembers(IEnumerable<TmdbCastMember> cast) =>
        cast
            .OrderBy(c => c.Order)
            .Take(TmdbConstants.MaxCastMembers)
            .Select(c => new CastMember
            {
                TmdbPersonId = c.Id,
                Name         = c.Name,
                Character    = c.Character,
                Order        = c.Order,
                ProfileUrl   = TmdbConstants.PosterUrl(c.ProfilePath)
            })
            .ToList();

    // ── ITmdbSearchService ───────────────────────────────────────────────────

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
                    await Task.Delay(TmdbConstants.ApiDelayMs);
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

    // ── ITmdbImportService ───────────────────────────────────────────────────

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

            var directorCrew = details.Credits.Crew.FirstOrDefault(c => c.Job == TmdbConstants.DirectorJob);
            var releaseYear  = int.TryParse(details.ReleaseDate?.Split('-')[0], out var year) ? year : 0;

            logger.LogInformation("TMDB import succeeded: \"{Title}\" ({Year}), director={Director}",
                details.Title, releaseYear, directorCrew?.Name ?? "unknown");

            return new Movie
            {
                Title          = details.Title,
                ReleaseYear    = releaseYear,
                Genre          = details.Genres.FirstOrDefault()?.Name,
                Director       = directorCrew?.Name,
                DirectorTmdbId = directorCrew?.Id,
                Rating         = (decimal)Math.Round(details.VoteAverage, 1),
                Synopsis       = details.Overview,
                PosterUrl      = TmdbConstants.PosterUrl(details.PosterPath),
                TmdbId         = details.Id,
                RuntimeMinutes = details.Runtime,
                CollectionId   = details.BelongsToCollection?.Id,
                CollectionName = details.BelongsToCollection?.Name,
                Cast           = MapCastMembers(details.Credits.Cast)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB import failed for tmdbId={TmdbId}", tmdbId);
            return null;
        }
    }

    // ── ITmdbCreditService ───────────────────────────────────────────────────

    public async Task<(List<CastMember> Cast, int? DirectorTmdbId)> FetchCreditsAsync(int tmdbId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}/credits?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return ([], null);

            var data = await response.Content.ReadFromJsonAsync<TmdbCredits>();
            if (data is null) return ([], null);

            var directorTmdbId = data.Crew.FirstOrDefault(c => c.Job == TmdbConstants.DirectorJob)?.Id;
            return (MapCastMembers(data.Cast), directorTmdbId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB credits fetch failed for tmdbId={TmdbId}", tmdbId);
            return ([], null);
        }
    }

    public async Task<List<CastMember>> FetchCastAsync(int tmdbId)
    {
        var (cast, _) = await FetchCreditsAsync(tmdbId);
        return cast;
    }

    // ── ITmdbMediaService ────────────────────────────────────────────────────

    public async Task<string?> GetTrailerKeyAsync(int tmdbId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}/videos?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
            return data?.Results
                .Where(v => v.Site == TmdbConstants.YouTubeSite && v.Type == TmdbConstants.TrailerType)
                .OrderByDescending(v => v.Official)
                .FirstOrDefault()?.Key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB videos fetch failed for tmdbId={TmdbId}", tmdbId);
            return null;
        }
    }

    public async Task<List<TmdbMovieResult>> GetRecommendationsAsync(int tmdbId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}/recommendations?api_key={_apiKey}&language=en-US&page=1");
            if (!response.IsSuccessStatusCode) return [];

            var data = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
            return data?.Results.Take(TmdbConstants.MaxRecommendations).ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB recommendations fetch failed for tmdbId={TmdbId}", tmdbId);
            return [];
        }
    }

    public async Task<TmdbWatchRegion?> GetWatchProvidersAsync(int tmdbId, string region = "US")
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"movie/{tmdbId}/watch/providers?api_key={_apiKey}");
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<TmdbWatchProvidersResponse>();
            if (data is null) return null;
            data.Results.TryGetValue(region, out var watchRegion);
            return watchRegion;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB watch providers fetch failed for tmdbId={TmdbId}", tmdbId);
            return null;
        }
    }

    public async Task<TmdbCollectionDetails?> GetCollectionAsync(int collectionId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"collection/{collectionId}?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TmdbCollectionDetails>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB collection fetch failed for collectionId={CollectionId}", collectionId);
            return null;
        }
    }

    public async Task<List<TmdbMovieResult>> GetTrendingAsync(string window = "week")
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"trending/movie/{window}?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return [];
            var data = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
            return data?.Results ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB trending fetch failed");
            return [];
        }
    }

    // ── ITmdbPersonService ───────────────────────────────────────────────────

    public async Task<TmdbPersonDetails?> GetPersonDetailsAsync(int personId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"person/{personId}?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TmdbPersonDetails>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB person details fetch failed for personId={PersonId}", personId);
            return null;
        }
    }

    public async Task<TmdbPersonMovieCreditsResponse?> GetPersonMovieCreditsAsync(int personId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"person/{personId}/movie_credits?api_key={_apiKey}&language=en-US");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TmdbPersonMovieCreditsResponse>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TMDB person credits fetch failed for personId={PersonId}", personId);
            return null;
        }
    }
}
