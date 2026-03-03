using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving all entities in a collection.
/// </summary>
public static class GetCustomCollectionEndpoint
{
    /// <summary>
    /// Maps the GET /{collection} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCustomCollection(this IEndpointRouteBuilder app)
        => app.MapGet("/{collection}", Handle)
            .WithName("GetCustomCollection")
            .WithSummary("Get all entities in a collection");

    private static Results<Ok<IEnumerable<JsonElement>>, NotFound<object>, UnauthorizedHttpResult> Handle(
        string collection,
        CustomCollectionService service,
        HttpContext httpContext)
    {
        if (!service.CollectionExists(collection))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{collection}' not found" });
        }

        if (service.IsAuthRequired(collection) && !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return TypedResults.Unauthorized();
        }

        var entities = service.GetAll(collection);
        return TypedResults.Ok(entities);
    }
}
