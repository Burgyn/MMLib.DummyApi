using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving a specific collection definition.
/// </summary>
public static class GetCollectionDefinitionEndpoint
{
    /// <summary>
    /// Maps the GET /_definitions/{name} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCollectionDefinition(this IEndpointRouteBuilder app)
        => app.MapGet("/_definitions/{name}", Handle)
            .WithName("GetCollectionDefinition")
            .WithSummary("Get a specific collection definition");

    private static Results<Ok<CollectionDefinition>, NotFound<object>> Handle(
        string name,
        CustomDataStore dataStore)
    {
        var definition = dataStore.GetDefinition(name);
        if (definition == null)
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        return TypedResults.Ok(definition);
    }
}
