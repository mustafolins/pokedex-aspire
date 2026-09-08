using pokedex_aspire.Models;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Characteristics
{
    private static readonly IReadOnlyList<string> StatOrder =
    [
        "hp",
        "attack",
        "defense",
        "special-attack",
        "special-defense",
        "speed"
    ];

    private IReadOnlyList<CharacteristicDto> Profiles { get; set; } = [];

    private string SelectedStat { get; set; } = StatOrder[0];

    private bool IsLoading { get; set; }

    private IReadOnlyList<string> AvailableStats => StatOrder
        .Where(stat => Profiles.Any(characteristic => characteristic.highest_stat?.name == stat))
        .ToList();

    private IReadOnlyList<CharacteristicDto> SelectedCharacteristics => Profiles
        .Where(characteristic => characteristic.highest_stat?.name == SelectedStat)
        .OrderBy(characteristic => characteristic.gene_modulo)
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Profiles = await pokemonRepository.GetCharacteristics();
        SelectedStat = AvailableStats.FirstOrDefault() ?? StatOrder[0];
        IsLoading = false;

        await base.OnInitializedAsync();
    }

    private void SelectStat(string stat)
    {
        SelectedStat = stat;
    }

    private static string GetEnglishDescription(CharacteristicDto characteristic)
    {
        return characteristic.descriptions?
            .FirstOrDefault(description => description.language?.name == "en")?
            .description ?? "Description unavailable";
    }

    private static string FormatStat(string stat)
    {
        return stat switch
        {
            "hp" => "HP",
            "special-attack" => "Special attack",
            "special-defense" => "Special defense",
            _ => stat.Replace('-', ' ')
        };
    }

    private static string GetStatCode(string stat)
    {
        return stat switch
        {
            "hp" => "HP",
            "attack" => "ATK",
            "defense" => "DEF",
            "special-attack" => "SPA",
            "special-defense" => "SPD",
            "speed" => "SPE",
            _ => "--"
        };
    }
}