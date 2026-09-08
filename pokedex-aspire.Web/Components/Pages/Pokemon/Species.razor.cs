using Microsoft.AspNetCore.Components;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Species
{
    [Parameter]
    public int PokemonId { get; set; }

    [Parameter]
    public EventCallback<int> PokemonSelected { get; set; }

    public SpecieDto? Specie { get; set; }

    private string Classification => Specie switch
    {
        { is_mythical: true } => "Mythical",
        { is_legendary: true } => "Legendary",
        { is_baby: true } => "Baby species",
        _ => "Standard species"
    };

    protected override async Task OnParametersSetAsync()
    {
        Specie = null;
        Specie = await pokemonRepository.GetSpecie(PokemonId);

        await base.OnParametersSetAsync();
    }
}