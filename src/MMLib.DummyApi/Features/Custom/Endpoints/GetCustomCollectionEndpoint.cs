using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCustomCollectionEndpoint
{
    public static RouteHandlerBuilder MapGetCustomCollection(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{collection}", Handle)
            .WithName("GetCustomCollection")
            .WithSummary("Get all entities in a collection");
    }

    private static HttpResults.Results<Ok<IEnumerable<JsonElement>>, NotFound<object>, UnauthorizedHttpResult> Handle(
        string collection,
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

        var entities = service.GetAll(collection);
        return TypedResults.Ok(entities);
    }
}
