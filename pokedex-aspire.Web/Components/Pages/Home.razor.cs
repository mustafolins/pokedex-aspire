using pokedex_aspire.Models;

namespace pokedex_aspire.Web.Components.Pages;

public partial class Home
{
    private static readonly PokedexView[] PokedexViews =
        [PokedexView.Scanner, PokedexView.Entry];

    private PokemonDto? pokemon;
    private int pokemonId = 1;
    private PokedexView activeView = PokedexView.Scanner;

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
        activeView = PokedexView.Entry;
        await LoadPokemon(id);
    }

    private static string GetViewLabel(PokedexView view) => view switch
    {
        PokedexView.Scanner => "Scanner",
        PokedexView.Entry => "Dex entry",
        _ => throw new ArgumentOutOfRangeException(nameof(view), view, null)
    };

    private async Task LoadPokemon(int id)
    {
        pokemon = await pokemonRepository.GetPokemon(id);

        StateHasChanged();
    }
}