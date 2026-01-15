using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetBackgroundConfigEndpoint
{
    public static RouteHandlerBuilder MapGetBackgroundConfig(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{collection}/_background", Handle)
            .WithName("GetBackgroundConfig")
            .WithSummary("Get the background job configuration for a collection");
    }

    private static HttpResults.Results<Ok<BackgroundJobConfig>, NotFound<object>> Handle(
        string collection,
        CustomCollectionService service)
    {
        var config = service.GetBackgroundConfig(collection);
        
        if (config == null)
        {
            return TypedResults.NotFound<object>(new { error = $"No background job configured for collection '{collection}'" });
        }

        return TypedResults.Ok(config);
    }
}
