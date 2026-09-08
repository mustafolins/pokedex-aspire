using KokoroSharp;
using KokoroSharp.Core;
using NAudio.Wave;

namespace pokedex_aspire.SpeechService;

public interface IPokemonSpeechSynthesizer
{
    Task<byte[]> SynthesizeAsync(string text);
}

public sealed class KokoroPokemonSpeechSynthesizer : IPokemonSpeechSynthesizer, IDisposable
{
    private const string ModelUrl =
        "https://github.com/Lyrcaxis/KokoroSharpBinaries/releases/download/v2.0.0/kokoro.onnx";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<KokoroPokemonSpeechSynthesizer> logger;
    private readonly Lazy<Task<KokoroWavSynthesizer>> synthesizer;
    private readonly SemaphoreSlim synthesisLock = new(1, 1);
    private readonly string modelPath;

    public KokoroPokemonSpeechSynthesizer(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<KokoroPokemonSpeechSynthesizer> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        modelPath = configuration["Speech:Kokoro:ModelPath"] ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "pokedex-aspire",
            "models",
            "kokoro.onnx");
        synthesizer = new Lazy<Task<KokoroWavSynthesizer>>(
            LoadSynthesizerAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<byte[]> SynthesizeAsync(string text)
    {
        await synthesisLock.WaitAsync();
        try
        {
            var wavSynthesizer = await synthesizer.Value;
            var voice = KokoroVoiceManager.GetVoice("af_sarah");
            var pcmBytes = await wavSynthesizer.SynthesizeAsync(text, voice);
            if (pcmBytes.Length == 0)
            {
                logger.LogWarning("Kokoro returned empty audio; retrying synthesis once.");
                pcmBytes = await wavSynthesizer.SynthesizeAsync(text, voice);
            }

            if (pcmBytes.Length == 0)
            {
                throw new InvalidOperationException("Kokoro returned empty audio after retrying synthesis.");
            }

            using var wavStream = new MemoryStream();
            using (var writer = new WaveFileWriter(wavStream, KokoroPlayback.waveFormat))
            {
                writer.Write(pcmBytes, 0, pcmBytes.Length);
            }

            return wavStream.ToArray();
        }
        finally
        {
            synthesisLock.Release();
        }
    }

    private async Task<KokoroWavSynthesizer> LoadSynthesizerAsync()
    {
        if (!File.Exists(modelPath))
        {
            await DownloadModelAsync();
        }

        logger.LogInformation("Loading Kokoro speech model from {ModelPath}", modelPath);
        return KokoroWavSynthesizer.LoadModel(modelPath);
    }

    private async Task DownloadModelAsync()
    {
        var modelDirectory = Path.GetDirectoryName(modelPath)
            ?? throw new InvalidOperationException("The Kokoro model path must include a directory.");
        Directory.CreateDirectory(modelDirectory);

        var temporaryPath = $"{modelPath}.{Guid.NewGuid():N}.tmp";
        logger.LogInformation("Downloading Kokoro speech model to {ModelPath}", modelPath);

        try
        {
            var httpClient = httpClientFactory.CreateClient("kokoro-model");
            using var response = await httpClient.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var source = await response.Content.ReadAsStreamAsync();
            await using var destination = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);
            await source.CopyToAsync(destination);
            await destination.FlushAsync();
            File.Move(temporaryPath, modelPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public void Dispose()
    {
        if (synthesizer.IsValueCreated && synthesizer.Value.IsCompletedSuccessfully)
        {
            synthesizer.Value.Result.Dispose();
        }

        synthesisLock.Dispose();
    }
}