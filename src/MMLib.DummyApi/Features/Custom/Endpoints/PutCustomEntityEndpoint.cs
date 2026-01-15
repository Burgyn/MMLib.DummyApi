using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class PutCustomEntityEndpoint
{
    public static RouteHandlerBuilder MapPutCustomEntity(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/{collection}/{id:guid}", Handle)
            .WithName("UpdateCustomEntity")
            .WithSummary("Update an entity in a collection");
    }

    private static HttpResults.Results<Ok<JsonElement>, NotFound<object>, BadRequest<object>, UnauthorizedHttpResult> Handle(
        string collection,
        Guid id,
        JsonElement data,
        CustomCollectionService service,
        HttpContext httpContext)
    {
        // Check if collection exists
        if (!service.CollectionExists(collection))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{collection}' not found" });
        }

        // Check auth if required
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
