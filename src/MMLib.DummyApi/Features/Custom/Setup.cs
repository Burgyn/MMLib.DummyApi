using MMLib.DummyApi.Features.Custom.Endpoints;

namespace MMLib.DummyApi.Features.Custom;

public static class Setup
{
    public static IServiceCollection AddCustomCollections(this IServiceCollection services)
    {
        services.AddSingleton<CustomDataStore>();
        services.AddSingleton<JsonSchemaValidator>();
        services.AddScoped<CustomCollectionService>();
        
        return services;
    }

    public static IEndpointRouteBuilder MapCustomCollections(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/custom")
            .WithTags("Custom");

        // Collection list
        group.MapGetCollections();

        // CRUD endpoints
        group.MapGetCustomCollection();
        group.MapGetCustomEntity();
        group.MapPostCustomEntity();
        group.MapPutCustomEntity();
        group.MapDeleteCustomEntity();

        // Schema endpoints
        group.MapGetSchema();
        group.MapPostSchema();
        group.MapDeleteSchema();

        // Background config endpoints
        group.MapGetBackgroundConfig();
        group.MapPostBackgroundConfig();
        group.MapDeleteBackgroundConfig();
        
        return app;
    }
}
