using Microsoft.AspNetCore.Components;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Encounters
{
    private const int LocationsPerPage = 6;

    [Parameter]
    public int PokemonId { get; set; }

    private IReadOnlyList<EncounterLocation> Locations { get; set; } = [];

    private int CurrentPage { get; set; } = 1;

    private bool IsLoading { get; set; }

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Locations.Count / (double)LocationsPerPage));

    private List<EncounterLocation> VisibleLocations => Locations
        .Skip((CurrentPage - 1) * LocationsPerPage)
        .Take(LocationsPerPage)
        .ToList();

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        Locations = [];
        CurrentPage = 1;

        var encounters = await pokemonRepository.GetPokemonEncounters(PokemonId);
        Locations = BuildLocations(encounters);

        IsLoading = false;
        await base.OnParametersSetAsync();
    }

    private int GetAreaNumber(EncounterLocation location)
    {
        return ((CurrentPage - 1) * LocationsPerPage) + VisibleLocations.IndexOf(location) + 1;
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private static IReadOnlyList<EncounterLocation> BuildLocations(List<EncounterDto>? encounters)
    {
        return (encounters ?? [])
            .Where(encounter => !string.IsNullOrWhiteSpace(encounter.location_area?.name))
            .Select(encounter =>
            {
                var versionDetails = encounter.version_details ?? [];
                var details = versionDetails
                    .SelectMany(version => version.encounter_details ?? [])
                    .ToList();
                var levels = details
                    .SelectMany(detail => new[] { detail.min_level, detail.max_level })
                    .Where(level => level > 0)
                    .ToList();
                var methods = details
                    .Select(detail => detail.method?.name)
                    .OfType<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new EncounterLocation(
                    encounter.location_area!.name!,
                    levels.Count > 0 ? levels.Min() : null,
                    levels.Count > 0 ? levels.Max() : null,
                    details.Count > 0 ? details.Max(detail => detail.chance) : 0,
                    versionDetails
                        .Select(version => version.version?.name)
                        .OfType<string>()
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count(),
                    methods);
            })
            .OrderBy(location => location.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string FormatName(string value)
    {
        return value.Replace('-', ' ');
    }

    private sealed record EncounterLocation(
        string Name,
        int? MinLevel,
        int? MaxLevel,
        int PeakChance,
        int VersionCount,
        IReadOnlyList<string> Methods)
    {
        public string LevelRange => (MinLevel, MaxLevel) switch
        {
            (int minLevel, int maxLevel) when minLevel == maxLevel => minLevel.ToString(),
            (int minLevel, int maxLevel) => $"{minLevel}-{maxLevel}",
            _ => "--"
        };
    }
}