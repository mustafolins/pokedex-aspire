using pokedex_aspire.SpeechService;

namespace pokedex_aspire.Tests;

public class PokemonNarrationTests
{
    [Test]
    public void BuildOverviewCreatesStableMetricBriefing()
    {
        var pokemon = new PokemonSpeechProfile
        {
            Id = 25,
            Name = "pikachu",
            Height = 4,
            Weight = 60,
            BaseExperience = 112,
            Moves = [new object(), new object()]
        };

        var narration = PokemonNarration.BuildOverview(pokemon);

        Assert.That(
            narration,
            Is.EqualTo(
                "Pokedex entry 25. pikachu. Height: 0.4 meters. Weight: 6 kilograms. " +
                "Base experience: 112. pikachu has 2 recorded moves."));
    }

    [Test]
    public void BuildSpeciesCreatesStableResearchBriefing()
    {
        var species = new PokemonSpeciesSpeechProfile
        {
            Name = "pikachu",
            BaseHappiness = 70,
            CaptureRate = 190,
            Color = new NamedPokemonResource { Name = "yellow" },
            Shape = new NamedPokemonResource { Name = "quadruped" },
            Habitat = new NamedPokemonResource { Name = "forest" },
            GrowthRate = new NamedPokemonResource { Name = "medium" },
            EggGroups =
            [
                new NamedPokemonResource { Name = "ground" },
                new NamedPokemonResource { Name = "fairy" }
            ]
        };

        var narration = PokemonNarration.BuildSpecies(species);

        Assert.That(
            narration,
            Is.EqualTo(
                "Species research. " +
                "pikachu is classified as a standard species. Its recorded color is yellow. " +
                "Its body shape is quadruped. Its habitat is forest. Growth rate: medium. " +
                "Base happiness: 70. Capture rate: 190. Egg groups are ground and fairy."));
    }
}