using BlazorWebAppMovies.Components;
using BlazorWebAppMovies.Data;
using BlazorWebAppMovies.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MovieDb")));

// Movie services — register concrete class once, expose through all three interfaces
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<IMovieService>(sp      => sp.GetRequiredService<MovieService>());
builder.Services.AddScoped<IMovieQueryService>(sp  => sp.GetRequiredService<MovieService>());
builder.Services.AddScoped<IMovieCommandService>(sp => sp.GetRequiredService<MovieService>());

builder.Services.AddScoped<IMovieImportOrchestrator, MovieImportOrchestrator>();
builder.Services.AddHostedService<CastBackfillService>();

// TMDB — typed HttpClient registered once; focused interfaces resolved from it
builder.Services.AddHttpClient<TmdbService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<ITmdbService>(sp        => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbSearchService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbImportService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbCreditService>(sp  => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbMediaService>(sp   => sp.GetRequiredService<TmdbService>());
builder.Services.AddScoped<ITmdbPersonService>(sp  => sp.GetRequiredService<TmdbService>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    db.Database.EnsureCreated();
}

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Seed after the server is running so the DI scope is fully initialised
app.Lifetime.ApplicationStarted.Register(() =>
    _ = Task.Run(() => MovieSeeder.SeedAsync(app.Services)));

app.Run();
