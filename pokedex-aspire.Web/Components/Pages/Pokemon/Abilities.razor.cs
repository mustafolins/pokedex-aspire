using Microsoft.AspNetCore.Components;
using pokedex_aspire.Models;
using pokedex_aspire.Services;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Abilities(IPokemonRepository pokemonRepository)
{
    [Parameter]
    public List<Ability>? abilities { get; set; }

    private IReadOnlyList<Ability> Assignments => abilities ?? [];

    private AbilityDetailDto? SelectedAbility { get; set; }

    private string? SelectedAssignmentName { get; set; }

    private bool IsLoading { get; set; }

    private string EnglishShortEffect => GetEnglishEffect()?.short_effect
        ?? "No effect summary is available.";

    private string EnglishEffect => GetEnglishEffect()?.effect
        ?? "No detailed effect is available.";

    protected override async Task OnParametersSetAsync()
    {
        var firstAbilityName = Assignments.FirstOrDefault()?.ability?.name;
        var selectedStillExists = Assignments.Any(
            assignment => assignment.ability?.name == SelectedAssignmentName);

        if (!selectedStillExists)
        {
            SelectedAbility = null;
            SelectedAssignmentName = null;

            if (!string.IsNullOrWhiteSpace(firstAbilityName))
            {
                await LoadAbilityAsync(firstAbilityName);
            }
        }

        await base.OnParametersSetAsync();
    }

    private async Task SelectAbilityAsync(Ability assignment)
    {
        var abilityName = assignment.ability?.name;
        if (!string.IsNullOrWhiteSpace(abilityName) && abilityName != SelectedAssignmentName)
        {
            await LoadAbilityAsync(abilityName);
        }
    }

    private async Task LoadAbilityAsync(string abilityName)
    {
        IsLoading = true;
        SelectedAssignmentName = abilityName;

        try
        {
            SelectedAbility = await pokemonRepository.GetAbility(abilityName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private EffectEntry? GetEnglishEffect()
    {
        return SelectedAbility?.effect_entries?
            .FirstOrDefault(effect => effect.language?.name == "en");
    }

    private static string FormatName(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unknown"
            : value.Replace('-', ' ');
    }

    private static string FormatGeneration(string? generation)
    {
        return generation?.Replace("generation-", "Gen ").ToUpperInvariant() ?? "Unknown";
    }
}