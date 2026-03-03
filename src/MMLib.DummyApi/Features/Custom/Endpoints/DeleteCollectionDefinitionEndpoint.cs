using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for deleting a collection definition and all its data.
/// </summary>
public static class DeleteCollectionDefinitionEndpoint
{
    /// <summary>
    /// Maps the DELETE /_definitions/{name} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapDeleteCollectionDefinition(this IEndpointRouteBuilder app)
        => app.MapDelete("/_definitions/{name}", Handle)
            .WithName("DeleteCollectionDefinition")
            .WithSummary("Delete a collection and all its data");

    private static Results<NoContent, NotFound<object>> Handle(
        string name,
        CustomDataStore dataStore)
    {
        if (!dataStore.DeleteDefinition(name))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        return TypedResults.NoContent();
    }
}
