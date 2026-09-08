using pokedex_aspire.SpeechService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("cache");

builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<IPokemonClient, PokemonClient>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https+http://apiservice");
});
builder.Services.AddHttpClient("kokoro-model");
builder.Services.AddSingleton<IPokemonSpeechSynthesizer, KokoroPokemonSpeechSynthesizer>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapPokemonSpeechEndpoints();

app.MapDefaultEndpoints();

app.Run();