using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom.Endpoints;
using MMLib.DummyApi.Features.Custom.Models;
using MMLib.DummyApi.Features.Custom.OpenApi;
using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Registration and mapping helpers for the Custom Collections feature.
/// </summary>
public static class Setup
{
    /// <summary>
    /// Registers Custom Collections feature services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddCustomCollections(this IServiceCollection services)
    {
        services.AddSingleton<CustomDataStore>();
        services.AddSingleton<JsonSchemaValidator>();
        services.AddSingleton<AutoBogusSeeder>();
        services.AddSingleton<RuleResolver>();
        services.AddScoped<CustomCollectionService>();

        return services;
    }

    /// <summary>
    /// Registers OpenAPI transformers for custom collections.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    public static OpenApiOptions AddCollectionOpenApiTransformers(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<CollectionOpenApiTransformer>();
        options.AddOperationTransformer<CollectionOpenApiTransformer>();
        return options;
    }

    /// <summary>
    /// Maps all Custom Collections management endpoints under /custom.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapCustomCollections(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/custom")
            .WithTags("Custom Collections");

        group.MapGetCollections();

        group.MapGetCollectionDefinitions();
        group.MapGetCollectionDefinition();
        group.MapPostCollectionDefinition();
        group.MapPutCollectionDefinition();
        group.MapDeleteCollectionDefinition();

        group.MapGetCustomCollection();
        group.MapGetCustomEntity();
        group.MapPostCustomEntity();
        group.MapPutCustomEntity();
        group.MapDeleteCustomEntity();

        return app;
    }

    /// <summary>
    /// Loads collections from the configured file and maps their dynamic endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static WebApplication LoadAndMapCollections(this WebApplication app)
    {
        var dataStore = app.Services.GetRequiredService<CustomDataStore>();
        var seeder = app.Services.GetRequiredService<AutoBogusSeeder>();
        var options = app.Services.GetRequiredService<IOptions<DummyApiOptions>>().Value;
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("MMLib.DummyApi.Features.Custom");

        var filePath = options.CollectionsFile;
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = Path.Combine(AppContext.BaseDirectory, "collections.json");
        }

        if (File.Exists(filePath))
        {
            logger.LogInformation("Loading collections from {FilePath}", filePath);
            var json = File.ReadAllText(filePath);
            var collectionsFile = JsonSerializer.Deserialize<CollectionsFile>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (collectionsFile?.Collections != null)
            {
                foreach (var definition in collectionsFile.Collections)
                {
                    logger.LogInformation("Loading collection '{Name}' with seedCount={SeedCount}",
                        definition.Name, definition.SeedCount);

                    dataStore.SaveDefinition(definition);

                    if (definition.SeedCount > 0)
                    {
                        var items = seeder.Generate(definition.Schema, definition.SeedCount);
                        foreach (var item in items)
                        {
                            dataStore.Add(definition.Name, item);
                        }
                        logger.LogInformation("Seeded {Count} items for collection '{Name}'", items.Count, definition.Name);
                    }

                    logger.LogInformation("Mapping endpoints for collection '{Name}'", definition.Name);
                    app.MapCollectionEndpoints(definition);
                }
            }
        }
        else
        {
            logger.LogInformation("No collections file found at {FilePath}", filePath);
        }

        return app;
    }
}
