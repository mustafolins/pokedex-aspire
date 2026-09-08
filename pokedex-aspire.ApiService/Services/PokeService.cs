using StackExchange.Redis;

namespace Services;

public interface IPokeService
{
    Task<string> GetAbility(string abilityName);
    Task<string> GetCharacteristic(int id);
    Task<string> GetCharacteristics();
    Task<string> GetEvolutionChain(int id);
    Task<string> GetMove(string moveName);
    Task<string> GetPokemon(int id);
    Task<string> GetPokemonEncounters(int id);
    Task<string> GetPokemonSpecies(int id);
}

public class PokeService(HttpClient httpClient, IConnectionMultiplexer connectionMux) : IPokeService
{
    public async Task<string> GetAbility(string abilityName)
    {
        var db = connectionMux.GetDatabase();
        var normalizedName = abilityName.Trim().ToLowerInvariant();
        var cachedResponse = await db.StringGetAsync($"ability:{normalizedName}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync($"ability/{Uri.EscapeDataString(normalizedName)}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"ability:{normalizedName}", responseContent);
        return responseContent;
    }

    public async Task<string> GetCharacteristics()
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync("characteristics");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync("characteristic?limit=100");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync("characteristics", responseContent);
        return responseContent;
    }

    public async Task<string> GetCharacteristic(int id)
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync($"characteristic:{id}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync($"characteristic/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"characteristic:{id}", responseContent);
        return responseContent;
    }

    public async Task<string> GetEvolutionChain(int id)
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync($"evolution-chain:{id}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync($"evolution-chain/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"evolution-chain:{id}", responseContent);
        return responseContent;
    }

    public async Task<string> GetPokemon(int id)
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync($"pokemon:{id}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }
        var response = await httpClient.GetAsync($"pokemon/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"pokemon:{id}", responseContent);
        return responseContent;
    }

    public async Task<string> GetPokemonEncounters(int id)
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync($"pokemon-encounters:{id}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync($"pokemon/{id}/encounters");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"pokemon-encounters:{id}", responseContent);
        return responseContent;
    }

    public async Task<string> GetMove(string moveName)
    {
        var db = connectionMux.GetDatabase();

        var cachedResponse = await db.StringGetAsync($"move:{moveName}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }

        var response = await httpClient.GetAsync($"move/{moveName}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"move:{moveName}", responseContent);
        return responseContent;
    }

    public async Task<string> GetPokemonSpecies(int id)
    {
        var db = connectionMux.GetDatabase();
        var cachedResponse = await db.StringGetAsync($"pokemon-species:{id}");
        if (!cachedResponse.IsNullOrEmpty)
        {
            return cachedResponse.ToString();
        }
        var response = await httpClient.GetAsync($"pokemon-species/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();
        await db.StringSetAsync($"pokemon-species:{id}", responseContent);
        return responseContent;
    }
}