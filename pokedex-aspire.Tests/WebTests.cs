using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace pokedex_aspire.Tests;

public class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Test]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetPokemonReturnsJsonObject()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/pokemon/1", cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(responseContent);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
            Assert.That(document.RootElement.GetProperty("id").GetInt32(), Is.EqualTo(1));
            Assert.That(document.RootElement.GetProperty("cries").GetProperty("latest").GetString(), Is.Not.Empty);
        });
    }

    [Test]
    public async Task GetAbilityReturnsEnglishEffect()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/ability/overgrow", cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var document = JsonDocument.Parse(responseContent);
        var englishEffect = document.RootElement
            .GetProperty("effect_entries")
            .EnumerateArray()
            .Single(effect => effect.GetProperty("language").GetProperty("name").GetString() == "en");

        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.GetProperty("name").GetString(), Is.EqualTo("overgrow"));
            Assert.That(document.RootElement.GetProperty("generation").GetProperty("name").GetString(), Is.EqualTo("generation-iii"));
            Assert.That(englishEffect.GetProperty("short_effect").GetString(), Does.Contain("Grass"));
        });
    }

    [Test]
    public async Task GetEvolutionChainReturnsNestedSpecies()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/evolution-chain/1", cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var document = JsonDocument.Parse(responseContent);
        var chain = document.RootElement.GetProperty("chain");
        var secondStage = chain.GetProperty("evolves_to")[0];
        var thirdStage = secondStage.GetProperty("evolves_to")[0];

        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.GetProperty("id").GetInt32(), Is.EqualTo(1));
            Assert.That(chain.GetProperty("species").GetProperty("name").GetString(), Is.EqualTo("bulbasaur"));
            Assert.That(secondStage.GetProperty("species").GetProperty("name").GetString(), Is.EqualTo("ivysaur"));
            Assert.That(thirdStage.GetProperty("species").GetProperty("name").GetString(), Is.EqualTo("venusaur"));
        });
    }

    [Test]
    public async Task GetPokemonEncountersReturnsLocationData()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/pokemon/1/encounters", cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var document = JsonDocument.Parse(responseContent);
        var firstEncounter = document.RootElement[0];

        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(document.RootElement.GetArrayLength(), Is.GreaterThan(0));
            Assert.That(firstEncounter.GetProperty("location_area").GetProperty("name").GetString(), Is.Not.Empty);
            Assert.That(firstEncounter.GetProperty("version_details").GetArrayLength(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task GetCharacteristicsReturnsIndexAndDetail()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var indexResponse = await httpClient.GetAsync("/characteristic", cancellationToken);
        var detailResponse = await httpClient.GetAsync("/characteristic/1", cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(indexResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });

        using var indexDocument = JsonDocument.Parse(await indexResponse.Content.ReadAsStringAsync(cancellationToken));
        using var detailDocument = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync(cancellationToken));
        var englishDescription = detailDocument.RootElement
            .GetProperty("descriptions")
            .EnumerateArray()
            .Single(description => description.GetProperty("language").GetProperty("name").GetString() == "en");

        Assert.Multiple(() =>
        {
            Assert.That(indexDocument.RootElement.GetProperty("count").GetInt32(), Is.EqualTo(30));
            Assert.That(indexDocument.RootElement.GetProperty("results").GetArrayLength(), Is.EqualTo(30));
            Assert.That(detailDocument.RootElement.GetProperty("highest_stat").GetProperty("name").GetString(), Is.EqualTo("hp"));
            Assert.That(detailDocument.RootElement.GetProperty("gene_modulo").GetInt32(), Is.EqualTo(0));
            Assert.That(detailDocument.RootElement.GetProperty("possible_values").GetArrayLength(), Is.GreaterThan(0));
            Assert.That(englishDescription.GetProperty("description").GetString(), Is.EqualTo("Loves to eat"));
        });
    }

    [Test]
    public async Task GetPokemonSpeechRejectsInvalidPokemonId()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("speechservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("speechservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var responses = await Task.WhenAll(
            httpClient.GetAsync("/speech/pokemon/0/overview-v1", cancellationToken),
            httpClient.GetAsync("/speech/pokemon/0/species-v1", cancellationToken));

        Assert.That(responses, Has.All.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task IdentifyPokemonRejectsUnsupportedMediaType()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.pokedex_aspire_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        using var content = new StringContent("not an image");
        content.Headers.ContentType = new("text/plain");
        var response = await httpClient.PostAsync("/vision/identify", content, cancellationToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }
}
