using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace pokedex_aspire.Web.Components.Pages.Pokemon;

public partial class PokemonScanner : IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public int PokemonId { get; set; }

    [Parameter]
    public EventCallback<int> PokemonSelected { get; set; }

    private ElementReference VideoElement { get; set; }

    private ElementReference CanvasElement { get; set; }

    private ElementReference FileInputElement { get; set; }

    private IJSObjectReference? Module { get; set; }

    private IReadOnlyList<PokemonPrediction> Predictions { get; set; } = [];

    private bool CameraActive { get; set; }

    private bool HasPreview { get; set; }

    private bool IsAnalyzing { get; set; }

    private string ModelStatus { get; set; } = "Initializing";

    private bool ModelReady { get; set; }

    private string? ErrorMessage { get; set; }

    private string ModelStatusClass => ModelReady ? "is-ready" : "is-warming";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Pages/Pokemon/PokemonScanner.razor.js");
            _ = WarmModelAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task WarmModelAsync()
    {
        if (Module is null)
        {
            return;
        }

        ModelStatus = "Model warming";
        await InvokeAsync(StateHasChanged);

        try
        {
            await Module.InvokeAsync<VisionReadiness>(
                "warmupAndWait",
                "/vision/warmup",
                "/vision/ready");
            ModelReady = true;
            ModelStatus = "Model ready";
        }
        catch (JSException)
        {
            ModelStatus = "Model offline";
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task StartCameraAsync()
    {
        if (Module is null)
        {
            return;
        }

        ErrorMessage = null;
        Predictions = [];
        HasPreview = false;

        try
        {
            await Module.InvokeVoidAsync("startCamera", VideoElement, CanvasElement);
            CameraActive = true;
        }
        catch (JSException error)
        {
            CameraActive = false;
            ErrorMessage = GetBrowserError(error, "Camera access failed.");
        }
    }

    private Task CaptureAsync()
    {
        return IdentifyAsync("captureAndIdentify", VideoElement, CanvasElement, "/vision/identify");
    }

    private Task IdentifyFileAsync()
    {
        return IdentifyAsync("identifyFile", FileInputElement, CanvasElement, "/vision/identify");
    }

    private async Task IdentifyAsync(string identifier, params object[] arguments)
    {
        if (Module is null || IsAnalyzing)
        {
            return;
        }

        IsAnalyzing = true;
        ErrorMessage = null;
        Predictions = [];

        try
        {
            var result = await Module.InvokeAsync<PokemonIdentificationResult>(identifier, arguments);
            Predictions = (result.Predictions ?? [])
                .Select((prediction, index) => prediction with { Rank = index + 1 })
                .ToList();
            HasPreview = true;
            CameraActive = false;
            ModelReady = true;
            ModelStatus = $"{result.Device} // {result.LatencyMs:0} ms";
        }
        catch (JSException error)
        {
            ErrorMessage = GetBrowserError(error, "Pokemon identification failed.");
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private async Task SelectPredictionAsync(PokemonPrediction prediction)
    {
        if (prediction.PokemonId is int pokemonId)
        {
            await PokemonSelected.InvokeAsync(pokemonId);
        }
    }

    private static string GetBrowserError(JSException error, string fallback)
    {
        var message = error.Message
            .Split("Error:", StringSplitOptions.TrimEntries)
            .LastOrDefault()?
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }

    public async ValueTask DisposeAsync()
    {
        if (Module is null)
        {
            return;
        }

        try
        {
            await Module.InvokeVoidAsync("dispose", VideoElement, CanvasElement);
            await Module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private sealed class PokemonIdentificationResult
    {
        [JsonPropertyName("modelRevision")]
        public string? ModelRevision { get; set; }

        [JsonPropertyName("device")]
        public string Device { get; set; } = "cpu";

        [JsonPropertyName("latencyMs")]
        public double LatencyMs { get; set; }

        [JsonPropertyName("predictions")]
        public List<PokemonPrediction>? Predictions { get; set; }
    }

    private sealed record PokemonPrediction
    {
        [JsonPropertyName("label")]
        public string Label { get; init; } = "Unknown";

        [JsonPropertyName("pokemonId")]
        public int? PokemonId { get; init; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; init; }

        [JsonPropertyName("supported")]
        public bool Supported { get; init; }

        public int Rank { get; init; }

        public bool CanSelect => Supported && Confidence >= 0.15;

        public string ActionLabel => !Supported
            ? "Event variant"
            : CanSelect ? "Load entry" : "Low confidence";

        public string ConfidenceWidth => string.Create(
            CultureInfo.InvariantCulture,
            $"{Math.Clamp(Confidence * 100, 0, 100):0.##}%");
    }

    private sealed class VisionReadiness
    {
        [JsonPropertyName("ready")]
        public bool Ready { get; set; }
    }
}