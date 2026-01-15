using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class DeleteCustomEntityEndpoint
{
    public static RouteHandlerBuilder MapDeleteCustomEntity(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{collection}/{id:guid}", Handle)
            .WithName("DeleteCustomEntity")
            .WithSummary("Delete an entity from a custom collection");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(
        string collection,
        Guid id,
        CustomCollectionService service)
    {
        var deleted = service.Delete(collection, id);
        if (!deleted)
        {
            return TypedResults.NotFound<object>(new { error = $"Entity not found in collection '{collection}'" });
        }

        return TypedResults.NoContent();
    }
}
