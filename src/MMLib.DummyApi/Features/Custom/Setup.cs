using System.Text.Json;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom.Endpoints;
using MMLib.DummyApi.Features.Custom.Models;
using MMLib.DummyApi.Features.Custom.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Features.Custom;

public static class Setup
{
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
    /// Register OpenAPI transformers for custom collections
    /// </summary>
    public static OpenApiOptions AddCollectionOpenApiTransformers(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<CollectionOpenApiTransformer>();
        options.AddOperationTransformer<CollectionOpenApiTransformer>();
        return options;
    }

    public static IEndpointRouteBuilder MapCustomCollections(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/custom")
            .WithTags("Custom Collections");

        // Collection list
        group.MapGetCollections();

        // Collection definitions management
        group.MapGetCollectionDefinitions();
        group.MapGetCollectionDefinition();
        group.MapPostCollectionDefinition();
        group.MapPutCollectionDefinition();
        group.MapDeleteCollectionDefinition();

        // CRUD endpoints for entities (generic fallback)
        group.MapGetCustomCollection();
        group.MapGetCustomEntity();
        group.MapPostCustomEntity();
        group.MapPutCustomEntity();
        group.MapDeleteCustomEntity();
        
        return app;
    }

    /// <summary>
    /// Load collections from file and map dynamic endpoints
    /// </summary>
    public static WebApplication LoadAndMapCollections(this WebApplication app)
    {
        var dataStore = app.Services.GetRequiredService<CustomDataStore>();
        var seeder = app.Services.GetRequiredService<AutoBogusSeeder>();
        var options = app.Services.GetRequiredService<IOptions<DummyApiOptions>>().Value;
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("MMLib.DummyApi.Features.Custom");

        // Get file path
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
                    
                    // Save definition
                    dataStore.SaveDefinition(definition);
                    
                    // Seed data if requested
                    if (definition.SeedCount > 0)
                    {
                        var items = seeder.Generate(definition.Schema, definition.SeedCount);
                        foreach (var item in items)
                        {
                            dataStore.Add(definition.Name, item);
                        }
                        logger.LogInformation("Seeded {Count} items for collection '{Name}'", items.Count, definition.Name);
                    }
                    
                    // Map dynamic endpoints for this collection
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
