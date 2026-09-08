using pokedex_aspire.Models;

namespace pokedex_aspire.Services;

public interface IPokemonRepository
{
    Task<AbilityDetailDto?> GetAbility(string abilityName);
    Task<IReadOnlyList<CharacteristicDto>> GetCharacteristics();
    Task<EvolutionChainDto?> GetEvolutionChain(int id);
    Task<MoveDto?> GetMove(string moveName);
    Task<PokemonDto?> GetPokemon(int id);
    Task<List<EncounterDto>?> GetPokemonEncounters(int id);
    Task<SpecieDto?> GetSpecie(int id);
}

public class PokemonRepository(HttpClient httpClient) : IPokemonRepository
{
    public async Task<AbilityDetailDto?> GetAbility(string abilityName)
    {
        return await httpClient.GetFromJsonAsync<AbilityDetailDto>(
            $"ability/{Uri.EscapeDataString(abilityName)}");
    }

    public async Task<IReadOnlyList<CharacteristicDto>> GetCharacteristics()
    {
        var index = await httpClient.GetFromJsonAsync<CharacteristicIndexDto>("characteristic");
        var characteristicIds = (index?.results ?? [])
            .Select(result => GetResourceId(result.url))
            .OfType<int>()
            .ToList();
        var characteristicTasks = characteristicIds
            .Select(id => httpClient.GetFromJsonAsync<CharacteristicDto>($"characteristic/{id}"));
        var characteristics = await Task.WhenAll(characteristicTasks);

        return characteristics
            .OfType<CharacteristicDto>()
            .OrderBy(characteristic => characteristic.id)
            .ToList();
    }

    public async Task<EvolutionChainDto?> GetEvolutionChain(int id)
    {
        return await httpClient.GetFromJsonAsync<EvolutionChainDto>($"evolution-chain/{id}");
    }

    public async Task<PokemonDto?> GetPokemon(int id)
    {
        return await httpClient.GetFromJsonAsync<PokemonDto>($"pokemon/{id}");
    }

    public async Task<List<EncounterDto>?> GetPokemonEncounters(int id)
    {
        return await httpClient.GetFromJsonAsync<List<EncounterDto>>($"pokemon/{id}/encounters");
    }

    public async Task<MoveDto?> GetMove(string moveName)
    {
        return await httpClient.GetFromJsonAsync<MoveDto>($"move/{moveName}");
    }

    public async Task<SpecieDto?> GetSpecie(int id)
    {
        return await httpClient.GetFromJsonAsync<SpecieDto>($"pokemon-species/{id}");
    }

    private static int? GetResourceId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return int.TryParse(url.TrimEnd('/').Split('/').Last(), out var resourceId)
            ? resourceId
            : null;
    }
}

