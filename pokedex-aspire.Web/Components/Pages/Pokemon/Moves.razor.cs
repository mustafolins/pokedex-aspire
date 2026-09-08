using Microsoft.AspNetCore.Components;
using pokedex_aspire.Models;
using pokedex_aspire.Services;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class Moves(IPokemonRepository pokeService)
{

    [Parameter]
    public List<Move>? moves { get; set; }

    public int MovesPerPage { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;

    public MoveDto? SelectedMove { get; set; }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void NextPage()
    {
        if (moves != null && (CurrentPage * MovesPerPage) < moves.Count)
        {
            CurrentPage++;
        }
    }

    public async Task OnMoveClickedAsync(Move move)
    {
        if (!string.IsNullOrEmpty(move.move?.name))
        {
            try
            {
                SelectedMove = await pokeService.GetMove(move.move.name ?? string.Empty);
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(move.move?.url))
                {
                    SelectedMove = null;
                }
                else
                {
                    SelectedMove = await pokeService.GetMove(move.move.url[0..^1].ToString().Split('/').Last());
                }
            }
        }

        StateHasChanged();
    }
}