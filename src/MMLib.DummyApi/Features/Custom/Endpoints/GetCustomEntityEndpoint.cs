using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving a specific entity from a collection.
/// </summary>
public static class GetCustomEntityEndpoint
{
    /// <summary>
    /// Maps the GET /{collection}/{id} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCustomEntity(this IEndpointRouteBuilder app)
        => app.MapGet("/{collection}/{id:guid}", Handle)
            .WithName("GetCustomEntity")
            .WithSummary("Get a specific entity from a collection");

    private static Results<Ok<JsonElement>, NotFound<object>, UnauthorizedHttpResult> Handle(
        string collection,
        Guid id,
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

        var entity = service.GetById(collection, id);
        if (entity == null)
        {
            return TypedResults.NotFound<object>(new { error = $"Entity not found in collection '{collection}'" });
        }

        return TypedResults.Ok(entity.Value);
    }
}
