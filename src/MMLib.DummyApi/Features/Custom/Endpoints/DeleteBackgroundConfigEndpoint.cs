using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class DeleteBackgroundConfigEndpoint
{
    public static RouteHandlerBuilder MapDeleteBackgroundConfig(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{collection}/_background", Handle)
            .WithName("DeleteBackgroundConfig")
            .WithSummary("Delete the background job configuration for a collection");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(
        string collection,
        CustomCollectionService service)
    {
        var deleted = service.DeleteBackgroundConfig(collection);
        
        if (!deleted)
        {
            return TypedResults.NotFound<object>(new { error = $"No background job configured for collection '{collection}'" });
        }

        return TypedResults.NoContent();
    }
}
