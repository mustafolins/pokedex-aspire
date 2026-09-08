using pokedex_aspire.Web;
using pokedex_aspire.Web.Components;
using pokedex_aspire.Services;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient("speechservice", httpClient =>
{
    httpClient.BaseAddress = new Uri("https+http://speechservice");
});
builder.Services.AddHttpClient("visionservice", httpClient =>
{
    httpClient.BaseAddress = new Uri("https+http://visionservice");
});

builder.Services.AddHttpClient<IPokemonRepository, PokemonRepository>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https+http://apiservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapGet("/speech/pokemon/{id:int}/{segment}", async Task<IResult> (
    int id,
    string segment,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    if (segment is not ("overview-v1" or "species-v1"))
    {
        return Results.BadRequest();
    }

    var httpClient = httpClientFactory.CreateClient("speechservice");
    using var response = await httpClient.GetAsync(
        $"speech/pokemon/{id}/{segment}",
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)response.StatusCode);
    }

    var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "audio/wav";
    return Results.File(audio, contentType, enableRangeProcessing: true);
})
.WithName("GetPokemonSpeechSegment");

app.MapGet("/vision/ready", async Task<IResult> (
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    var httpClient = httpClientFactory.CreateClient("visionservice");
    using var response = await httpClient.GetAsync("ready", cancellationToken);
    return await ToProxyResultAsync(response, cancellationToken);
})
.WithName("GetVisionReadiness");

app.MapPost("/vision/warmup", async Task<IResult> (
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    var httpClient = httpClientFactory.CreateClient("visionservice");
    using var response = await httpClient.PostAsync("warmup", content: null, cancellationToken);
    return await ToProxyResultAsync(response, cancellationToken);
})
.WithName("WarmVisionModel");

app.MapPost("/vision/identify", async Task<IResult> (
    HttpRequest request,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    if (request.ContentType is null ||
        !request.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
    {
        return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
    }

    if (request.ContentLength > 6 * 1024 * 1024)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    using var requestBuffer = new MemoryStream();
    await request.Body.CopyToAsync(requestBuffer, cancellationToken);
    if (requestBuffer.Length > 6 * 1024 * 1024)
    {
        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    using var content = new ByteArrayContent(requestBuffer.ToArray());
    content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);

    var httpClient = httpClientFactory.CreateClient("visionservice");
    using var response = await httpClient.PostAsync("identify", content, cancellationToken);
    return await ToProxyResultAsync(response, cancellationToken);
})
.DisableAntiforgery()
.WithName("IdentifyPokemon");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

static async Task<IResult> ToProxyResultAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    var payload = await response.Content.ReadAsStringAsync(cancellationToken);
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    return Results.Content(
        payload,
        contentType,
        Encoding.UTF8,
        (int)response.StatusCode);
}
