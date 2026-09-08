var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.pokedex_aspire_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache);

var speechService = builder.AddProject<Projects.pokedex_aspire_SpeechService>("speechservice")
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

var visionService = builder.AddUvicornApp(
        name: "visionservice",
        appDirectory: "../pokedex-aspire.VisionService",
        app: "main:app")
    .WithUv()
    .WithHttpEndpoint(port: 8000, env: "PORT")
    .WithEnvironment("UVICORN_HOST", "0.0.0.0")
    .WithEnvironment("UVICORN_WORKERS", "1")
    .WithEnvironment("VISION_DEVICE", "cpu")
    .WithEnvironment(
        "HF_HOME",
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "pokedex-aspire",
            "models",
            "huggingface"))
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.pokedex_aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(speechService)
    .WaitFor(speechService)
    .WithReference(visionService)
    .WaitFor(visionService);

builder.Build().Run();
