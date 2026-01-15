using MMLib.DummyApi.Features.Custom.Endpoints;
using MMLib.DummyApi.Features.Custom.OpenApi;
using Microsoft.AspNetCore.OpenApi;

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
}
