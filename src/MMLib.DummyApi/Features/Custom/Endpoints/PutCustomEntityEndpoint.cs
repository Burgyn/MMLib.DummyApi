using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for updating an entity in a collection.
/// </summary>
public static class PutCustomEntityEndpoint
{
    /// <summary>
    /// Maps the PUT /{collection}/{id} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapPutCustomEntity(this IEndpointRouteBuilder app)
        => app.MapPut("/{collection}/{id:guid}", Handle)
            .WithName("UpdateCustomEntity")
            .WithSummary("Update an entity in a collection");

    private static Results<Ok<JsonElement>, NotFound<object>, BadRequest<object>, UnauthorizedHttpResult> Handle(
        string collection,
        Guid id,
        JsonElement data,
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

        var (entity, errors) = service.Update(collection, id, data);

        if (entity == null)
        {
            if (errors.Any(e => e.Contains("not found")))
            {
                return TypedResults.NotFound<object>(new { error = $"Entity not found in collection '{collection}'" });
            }
            return TypedResults.BadRequest<object>(new { errors });
        }

        return TypedResults.Ok(entity.Value);
    }
}
