namespace BlazorWebAppMovies.Components.Pages.Movies;

internal sealed class MovieFilterState
{
    public string   Search       { get; set; } = "";
    public string   Genre        { get; set; } = "";
    public string   WatchStatus  { get; set; } = "";
    public string   SortBy       { get; set; } = "title";
    public bool     Ascending    { get; set; } = true;
    public int      Page         { get; set; } = 1;
    public int      PageSize     { get; set; } = 25;
    public bool     FavoritesOnly { get; set; }
    public int?     YearFrom     { get; set; }
    public int?     YearTo       { get; set; }
    public decimal? RatingMin    { get; set; }
    public int?     CollectionId { get; set; }
    public string?  Tag          { get; set; }
    public int?     RuntimeMin   { get; set; }
    public int?     RuntimeMax   { get; set; }

    public Dictionary<string, object?> ToQueryParams() => new()
    {
        ["search"]     = string.IsNullOrEmpty(Search)      ? null : Search,
        ["genre"]      = string.IsNullOrEmpty(Genre)        ? null : Genre,
        ["status"]     = string.IsNullOrEmpty(WatchStatus)  ? null : WatchStatus,
        ["sort"]       = SortBy == "title"                  ? null : SortBy,
        ["asc"]        = Ascending                          ? null : (object?)false,
        ["page"]       = Page == 1                          ? null : (object?)Page,
        ["size"]       = PageSize == 25                     ? null : (object?)PageSize,
        ["favorites"]  = FavoritesOnly                      ? (object?)true : null,
        ["yfrom"]      = YearFrom,
        ["yto"]        = YearTo,
        ["rmin"]       = RatingMin,
        ["collection"] = CollectionId,
        ["tag"]        = Tag,
        ["rtmin"]      = RuntimeMin,
        ["rtmax"]      = RuntimeMax,
    };
}
