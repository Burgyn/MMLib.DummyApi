using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for creating a new collection definition.
/// </summary>
public static class PostCollectionDefinitionEndpoint
{
    /// <summary>
    /// Maps the POST /_definitions endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapPostCollectionDefinition(this IEndpointRouteBuilder app)
        => app.MapPost("/_definitions", Handle)
            .WithName("CreateCollectionDefinition")
            .WithSummary("Create a new collection with definition");

    private static Results<Created<CollectionDefinition>, BadRequest<object>, Conflict<object>> Handle(
        CollectionDefinition definition,
        CustomDataStore dataStore,
        AutoBogusSeeder seeder)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            return TypedResults.BadRequest<object>(new { error = "Collection name is required" });
        }

        if (dataStore.CollectionExists(definition.Name))
        {
            return TypedResults.Conflict<object>(new { error = $"Collection '{definition.Name}' already exists" });
        }

        dataStore.SaveDefinition(definition);

        if (definition.SeedCount > 0)
        {
            var items = seeder.Generate(definition.Schema, definition.SeedCount);
            foreach (var item in items)
            {
                dataStore.Add(definition.Name, item);
            }
        }

        return TypedResults.Created($"/custom/_definitions/{definition.Name}", definition);
    }
}
