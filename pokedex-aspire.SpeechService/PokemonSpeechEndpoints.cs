using StackExchange.Redis;

namespace pokedex_aspire.SpeechService;

public static class PokemonSpeechEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(30);

    public static IEndpointRouteBuilder MapPokemonSpeechEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/speech/pokemon/{id:int}/overview-v1", GetOverviewAsync)
            .WithName("GetPokemonOverviewSpeech");
        endpoints.MapGet("/speech/pokemon/{id:int}/species-v1", GetSpeciesAsync)
            .WithName("GetPokemonSpeciesSpeech");

        return endpoints;
    }

    private static Task<IResult> GetOverviewAsync(
        int id,
        IPokemonClient pokemonClient,
        IPokemonSpeechSynthesizer speechSynthesizer,
        IConnectionMultiplexer connectionMultiplexer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        return GetOrCreateAudioAsync(
            id,
            "overview-v1",
            async () =>
            {
                var pokemon = await pokemonClient.GetPokemonAsync(id, cancellationToken);
                return pokemon is null ? null : PokemonNarration.BuildOverview(pokemon);
            },
            speechSynthesizer,
            connectionMultiplexer,
            httpContext);
    }

    private static Task<IResult> GetSpeciesAsync(
        int id,
        IPokemonClient pokemonClient,
        IPokemonSpeechSynthesizer speechSynthesizer,
        IConnectionMultiplexer connectionMultiplexer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        return GetOrCreateAudioAsync(
            id,
            "species-research-v1",
            async () =>
            {
                var species = await pokemonClient.GetPokemonSpeciesAsync(id, cancellationToken);
                return species is null ? null : PokemonNarration.BuildSpecies(species);
            },
            speechSynthesizer,
            connectionMultiplexer,
            httpContext);
    }

    private static async Task<IResult> GetOrCreateAudioAsync(
        int id,
        string segment,
        Func<Task<string?>> getNarration,
        IPokemonSpeechSynthesizer speechSynthesizer,
        IConnectionMultiplexer connectionMultiplexer,
        HttpContext httpContext)
    {
        if (id < 1)
        {
            return Results.BadRequest();
        }

        var database = connectionMultiplexer.GetDatabase();
        var cacheKey = $"pokemon-speech:{segment}:af-sarah:{id}";
        var cachedAudio = await database.StringGetAsync(cacheKey);
        if (!cachedAudio.IsNullOrEmpty)
        {
            httpContext.Response.Headers["X-Speech-Cache"] = "HIT";
            return Results.File((byte[])cachedAudio!, "audio/wav", enableRangeProcessing: true);
        }

        var narration = await getNarration();
        if (narration is null)
        {
            return Results.NotFound();
        }

        var audio = await speechSynthesizer.SynthesizeAsync(narration);
        var cached = await database.StringSetAsync(cacheKey, audio, CacheDuration);
        if (!cached)
        {
            throw new InvalidOperationException("Redis rejected the generated Pokemon speech cache entry.");
        }

        httpContext.Response.Headers["X-Speech-Cache"] = "MISS";
        return Results.File(audio, "audio/wav", enableRangeProcessing: true);
    }
}