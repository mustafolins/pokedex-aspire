using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using pokedex_aspire.Models;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class PokemonSpeech : IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter, EditorRequired]
    public PokemonDto Pokemon { get; set; } = default!;

    private ElementReference CryAudioElement { get; set; }

    private ElementReference OverviewAudioElement { get; set; }

    private ElementReference SpeciesAudioElement { get; set; }

    private IJSObjectReference? AudioModule { get; set; }

    private int? PreviousPokemonId { get; set; }

    private bool IsSpeaking { get; set; }

    private bool IsPreparing { get; set; }

    private bool HasError { get; set; }

    private string StatusMessage { get; set; } = "Initializing";

    private string DisplayName => FormatName(Pokemon.name);

    private string? CryAudioSource => Pokemon.cries?.latest;

    private string OverviewAudioSource => $"/speech/pokemon/{Pokemon.id}/overview-v1";

    private string SpeciesAudioSource => $"/speech/pokemon/{Pokemon.id}/species-v1";

    private bool IsActive => IsPreparing || IsSpeaking;

    private bool IsReadDisabled => AudioModule is null || IsActive;

    private string StatusClass => HasError
        ? "is-error"
        : IsSpeaking ? "is-speaking" : string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (PreviousPokemonId is not null && PreviousPokemonId != Pokemon.id && IsActive)
        {
            await StopBriefingAsync();
        }

        PreviousPokemonId = Pokemon.id;
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            AudioModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Pages/Pokemon/PokemonSpeech.razor.js");
            StatusMessage = "Ready";
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task PlayBriefingAsync()
    {
        if (AudioModule is null)
        {
            return;
        }

        HasError = false;
        IsPreparing = true;
        StatusMessage = string.IsNullOrWhiteSpace(CryAudioSource)
            ? "Preparing overview"
            : "Preparing cry";
        StateHasChanged();

        try
        {
            await AudioModule.InvokeVoidAsync(
                "playSequence",
                CryAudioElement,
                OverviewAudioElement,
                SpeciesAudioElement);
        }
        catch (JSException) when (!IsPreparing && !IsSpeaking)
        {
        }
        catch (JSException)
        {
            CompleteAudio("Audio error", hasError: true);
        }
        finally
        {
            IsPreparing = false;
        }
    }

    private async Task StopBriefingAsync()
    {
        if (AudioModule is null)
        {
            return;
        }

        CompleteAudio("Ready");

        try
        {
            await AudioModule.InvokeVoidAsync(
                "stopSequence",
                CryAudioElement,
                OverviewAudioElement,
                SpeciesAudioElement);
        }
        catch (JSException)
        {
            CompleteAudio("Audio error", hasError: true);
        }
    }

    private void OnCryPlaying()
    {
        UpdateAudioState(isPreparing: false, isSpeaking: true, "Playing cry");
    }

    private void OnCryEnded()
    {
        UpdateAudioState(isPreparing: true, isSpeaking: false, "Preparing overview");
    }

    private void OnCryError()
    {
        UpdateAudioState(isPreparing: true, isSpeaking: false, "Preparing overview");
    }

    private void OnOverviewPlaying()
    {
        UpdateAudioState(isPreparing: false, isSpeaking: true, "Speaking overview");
    }

    private void OnOverviewEnded()
    {
        UpdateAudioState(isPreparing: true, isSpeaking: false, "Preparing research");
    }

    private void OnSpeciesPlaying()
    {
        UpdateAudioState(isPreparing: false, isSpeaking: true, "Speaking research");
    }

    private void OnSpeciesEnded()
    {
        CompleteAudio("Ready");
    }

    private void OnAudioError()
    {
        CompleteAudio("Audio error", hasError: true);
    }

    private void UpdateAudioState(
        bool isPreparing,
        bool isSpeaking,
        string message,
        bool hasError = false)
    {
        IsSpeaking = isSpeaking;
        IsPreparing = isPreparing;
        HasError = hasError;
        StatusMessage = message;
    }

    private void CompleteAudio(string message, bool hasError = false)
    {
        UpdateAudioState(isPreparing: false, isSpeaking: false, message, hasError);
    }

    private static string FormatName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? "Unknown Pokemon"
            : name.Replace('-', ' ');
    }

    public async ValueTask DisposeAsync()
    {
        if (AudioModule is not null)
        {
            try
            {
                await AudioModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}