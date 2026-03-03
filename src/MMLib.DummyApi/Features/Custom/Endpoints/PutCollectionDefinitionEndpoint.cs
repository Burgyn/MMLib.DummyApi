using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for updating a collection definition.
/// </summary>
public static class PutCollectionDefinitionEndpoint
{
    /// <summary>
    /// Maps the PUT /_definitions/{name} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapPutCollectionDefinition(this IEndpointRouteBuilder app)
        => app.MapPut("/_definitions/{name}", Handle)
            .WithName("UpdateCollectionDefinition")
            .WithSummary("Update a collection definition (does not affect existing data)");

    private static Results<Ok<CollectionDefinition>, NotFound<object>, BadRequest<object>> Handle(
        string name,
        CollectionDefinition definition,
        CustomDataStore dataStore)
    {
        if (!dataStore.CollectionExists(name))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        var updatedDefinition = definition with { Name = name };
        dataStore.SaveDefinition(updatedDefinition);

        return TypedResults.Ok(updatedDefinition);
    }
}
