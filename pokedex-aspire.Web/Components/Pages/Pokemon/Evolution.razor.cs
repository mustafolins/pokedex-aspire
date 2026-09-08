using Microsoft.AspNetCore.Components;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Evolution
{
    [Parameter]
    public string? EvolutionChainUrl { get; set; }

    [Parameter]
    public int PokemonId { get; set; }

    [Parameter]
    public EventCallback<int> PokemonSelected { get; set; }

    private EvolutionChainDto? EvolutionChain { get; set; }

    private IReadOnlyList<EvolutionStage> Stages { get; set; } = [];

    private bool IsLoading { get; set; }

    private string EvolutionChainLabel => EvolutionChain?.id is int chainId
        ? $"Chain #{chainId}"
        : "Chain unavailable";

    protected override async Task OnParametersSetAsync()
    {
        EvolutionChain = null;
        Stages = [];
        IsLoading = true;

        var evolutionChainId = GetResourceId(EvolutionChainUrl);
        if (evolutionChainId is not null)
        {
            EvolutionChain = await pokemonRepository.GetEvolutionChain(evolutionChainId.Value);
            Stages = BuildStages(EvolutionChain);
        }

        IsLoading = false;
        await base.OnParametersSetAsync();
    }

    private async Task SelectEvolutionAsync(EvolutionEntry entry)
    {
        if (entry.PokemonId is int pokemonId)
        {
            await PokemonSelected.InvokeAsync(pokemonId);
        }
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

    private static IReadOnlyList<EvolutionStage> BuildStages(EvolutionChainDto? evolutionChain)
    {
        if (evolutionChain?.chain is null)
        {
            return [];
        }

        var stages = new SortedDictionary<int, List<EvolutionEntry>>();
        AddEvolutionLink(
            stages,
            1,
            evolutionChain.chain.species?.name,
            evolutionChain.chain.species?.url,
            evolutionChain.chain.is_baby,
            evolutionChain.chain.evolution_details,
            evolutionChain.chain.evolves_to);

        return stages
            .Select(stage => new EvolutionStage(stage.Key, stage.Value))
            .ToList();
    }

    private static void AddEvolutionLink(
        IDictionary<int, List<EvolutionEntry>> stages,
        int stageNumber,
        string? speciesName,
        string? speciesUrl,
        bool isBaby,
        List<EvolutionDetail>? details,
        List<EvolvesTo>? evolutions)
    {
        if (!stages.TryGetValue(stageNumber, out var stageEntries))
        {
            stageEntries = [];
            stages[stageNumber] = stageEntries;
        }

        stageEntries.Add(new EvolutionEntry(
            GetResourceId(speciesUrl),
            speciesName ?? "Unknown species",
            FormatRequirement(stageNumber, isBaby, details)));

        foreach (var evolution in evolutions ?? [])
        {
            AddEvolutionLink(
                stages,
                stageNumber + 1,
                evolution.species?.name,
                evolution.species?.url,
                evolution.is_baby,
                evolution.evolution_details,
                evolution.evolves_to);
        }
    }

    private static string FormatRequirement(
        int stageNumber,
        bool isBaby,
        List<EvolutionDetail>? details)
    {
        if (stageNumber == 1)
        {
            return isBaby ? "Baby form" : "Base form";
        }

        var detail = details?.FirstOrDefault();
        if (detail?.min_level > 0)
        {
            return $"Level {detail.min_level}";
        }

        return detail?.trigger?.name switch
        {
            "level-up" => "Level up",
            "trade" => "Trade",
            "use-item" => "Use item",
            { Length: > 0 } trigger => trigger.Replace('-', ' '),
            _ => "Special condition"
        };
    }

    private sealed record EvolutionStage(int Number, IReadOnlyList<EvolutionEntry> Entries);

    private sealed record EvolutionEntry(int? PokemonId, string Name, string Requirement);
}