using System.Text.Json.Serialization;

namespace pokedex_aspire.SpeechService;

public interface IPokemonClient
{
    Task<PokemonSpeechProfile?> GetPokemonAsync(int id, CancellationToken cancellationToken);
    Task<PokemonSpeciesSpeechProfile?> GetPokemonSpeciesAsync(int id, CancellationToken cancellationToken);
}

public sealed class PokemonClient(HttpClient httpClient) : IPokemonClient
{
    public Task<PokemonSpeechProfile?> GetPokemonAsync(int id, CancellationToken cancellationToken)
    {
        return httpClient.GetFromJsonAsync<PokemonSpeechProfile>($"pokemon/{id}", cancellationToken);
    }

    public Task<PokemonSpeciesSpeechProfile?> GetPokemonSpeciesAsync(int id, CancellationToken cancellationToken)
    {
        return httpClient.GetFromJsonAsync<PokemonSpeciesSpeechProfile>($"pokemon-species/{id}", cancellationToken);
    }
}

public sealed class PokemonSpeechProfile
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("base_experience")]
    public int BaseExperience { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("moves")]
    public List<object>? Moves { get; set; }
}

public sealed class PokemonSpeciesSpeechProfile
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("base_happiness")]
    public int BaseHappiness { get; set; }

    [JsonPropertyName("capture_rate")]
    public int CaptureRate { get; set; }

    [JsonPropertyName("is_baby")]
    public bool IsBaby { get; set; }

    [JsonPropertyName("is_legendary")]
    public bool IsLegendary { get; set; }

    [JsonPropertyName("is_mythical")]
    public bool IsMythical { get; set; }

    [JsonPropertyName("color")]
    public NamedPokemonResource? Color { get; set; }

    [JsonPropertyName("shape")]
    public NamedPokemonResource? Shape { get; set; }

    [JsonPropertyName("habitat")]
    public NamedPokemonResource? Habitat { get; set; }

    [JsonPropertyName("growth_rate")]
    public NamedPokemonResource? GrowthRate { get; set; }

    [JsonPropertyName("egg_groups")]
    public List<NamedPokemonResource>? EggGroups { get; set; }
}

public sealed class NamedPokemonResource
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}