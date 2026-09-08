using System.Globalization;

namespace pokedex_aspire.SpeechService;

public static class PokemonNarration
{
    public static string BuildOverview(PokemonSpeechProfile pokemon)
    {
        var name = FormatName(pokemon.Name);
        var heightInMeters = pokemon.Height / 10d;
        var weightInKilograms = pokemon.Weight / 10d;
        var moveCount = pokemon.Moves?.Count ?? 0;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"Pokedex entry {pokemon.Id}. {name}. Height: {heightInMeters:0.#} meters. " +
            $"Weight: {weightInKilograms:0.#} kilograms. Base experience: {pokemon.BaseExperience}. " +
            $"{name} has {moveCount} recorded moves.");
    }

    public static string BuildSpecies(PokemonSpeciesSpeechProfile species)
    {
        var name = FormatName(species.Name);
        var research = new List<string>
        {
            "Species research.",
            $"{name} is classified as {GetClassification(species)}."
        };

        AddNamedFact(research, "Its recorded color is", species.Color?.Name);
        AddNamedFact(research, "Its body shape is", species.Shape?.Name);
        AddNamedFact(research, "Its habitat is", species.Habitat?.Name);
        AddNamedFact(research, "Growth rate:", species.GrowthRate?.Name);
        research.Add($"Base happiness: {species.BaseHappiness}.");
        research.Add($"Capture rate: {species.CaptureRate}.");

        var eggGroups = (species.EggGroups ?? [])
            .Select(eggGroup => FormatName(eggGroup.Name))
            .Where(eggGroup => !string.IsNullOrWhiteSpace(eggGroup))
            .ToList();
        if (eggGroups.Count > 0)
        {
            research.Add($"Egg groups are {JoinNaturally(eggGroups)}.");
        }

        return string.Join(' ', research);
    }

    private static string GetClassification(PokemonSpeciesSpeechProfile species)
    {
        return species switch
        {
            { IsMythical: true } => "a mythical Pokemon",
            { IsLegendary: true } => "a legendary Pokemon",
            { IsBaby: true } => "a baby Pokemon",
            _ => "a standard species"
        };
    }

    private static void AddNamedFact(ICollection<string> facts, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            facts.Add($"{label} {FormatName(value)}.");
        }
    }

    private static string JoinNaturally(IReadOnlyList<string> values)
    {
        return values.Count switch
        {
            1 => values[0],
            2 => $"{values[0]} and {values[1]}",
            _ => $"{string.Join(", ", values.Take(values.Count - 1))}, and {values[^1]}"
        };
    }

    private static string FormatName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? "Unknown Pokemon"
            : name.Replace('-', ' ');
    }
}