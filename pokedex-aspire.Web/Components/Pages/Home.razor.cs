using pokedex_aspire.Models;

namespace pokedex_aspire.Web.Components.Pages;

public partial class Home
{

    private PokemonDto? pokemon;
    private int pokemonId = 1;

    public int PokemonId
    {
        get => pokemonId;
        set
        {
            pokemonId = value;
            _ = LoadPokemon(pokemonId);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadPokemon(PokemonId);
    }

    private async Task LoadPokemonFromSelectionAsync(int id)
    {
        pokemonId = id;
        pokemon = null;
        await LoadPokemon(id);
    }

    private async Task LoadPokemon(int id)
    {
        pokemon = await pokemonRepository.GetPokemon(id);

        StateHasChanged();
    }
}