using Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add redis cache integration.
builder.AddRedisClient("cache");

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(
    new HttpClient
    {
        BaseAddress = new Uri("https://pokeapi.co/api/v2/")
    });
builder.Services.AddSingleton<IPokeService, PokeService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/ability/{abilityName}", async (string abilityName, IPokeService pokeService) =>
{
    var result = await pokeService.GetAbility(abilityName);
    return Results.Content(result, "application/json");
})
.WithName("GetAbility");

app.MapGet("/characteristic", async (IPokeService pokeService) =>
{
    var result = await pokeService.GetCharacteristics();
    return Results.Content(result, "application/json");
})
.WithName("GetCharacteristics");

app.MapGet("/characteristic/{id}", async (int id, IPokeService pokeService) =>
{
    var result = await pokeService.GetCharacteristic(id);
    return Results.Content(result, "application/json");
})
.WithName("GetCharacteristic");

app.MapGet("/pokemon/{id}", async (int id, IPokeService pokeService) =>
{
    var result = await pokeService.GetPokemon(id);
    return Results.Content(result, "application/json");
})
.WithName("GetPokemon");

app.MapGet("/pokemon/{id}/encounters", async (int id, IPokeService pokeService) =>
{
    var result = await pokeService.GetPokemonEncounters(id);
    return Results.Content(result, "application/json");
})
.WithName("GetPokemonEncounters");

app.MapGet("/move/{moveName}", async (string moveName, IPokeService pokeService) =>
{
    var result = await pokeService.GetMove(moveName);
    return Results.Content(result, "application/json");
})
.WithName("GetMove");

app.MapGet("/pokemon-species/{id}", async (int id, IPokeService pokeService) =>
{
    var result = await pokeService.GetPokemonSpecies(id);
    return Results.Content(result, "application/json");
})
.WithName("GetPokemonSpecies");

app.MapGet("/evolution-chain/{id}", async (int id, IPokeService pokeService) =>
{
    var result = await pokeService.GetEvolutionChain(id);
    return Results.Content(result, "application/json");
})
.WithName("GetEvolutionChain");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
