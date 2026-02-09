using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class DeleteCustomEntityEndpoint
{
    public static RouteHandlerBuilder MapDeleteCustomEntity(this IEndpointRouteBuilder app)
        => app.MapDelete("/{collection}/{id:guid}", Handle)
            .WithName("DeleteCustomEntity")
            .WithSummary("Delete an entity from a collection");

    private static HttpResults.Results<NoContent, NotFound<object>, UnauthorizedHttpResult> Handle(
        string collection,
        Guid id,
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

        if (!service.Delete(collection, id))
        {
            return TypedResults.NotFound<object>(new { error = $"Entity not found in collection '{collection}'" });
        }

        return TypedResults.NoContent();
    }
}
