using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCustomEntityEndpoint
{
    public static RouteHandlerBuilder MapGetCustomEntity(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{collection}/{id:guid}", Handle)
            .WithName("GetCustomEntity")
            .WithSummary("Get a specific entity from a custom collection");
    }

    private static HttpResults.Results<Ok<JsonElement>, NotFound<object>> Handle(
        string collection,
        Guid id,
        CustomCollectionService service)
    {
        var entity = service.GetById(collection, id);
        if (entity == null)
        {
            return TypedResults.NotFound<object>(new { error = $"Entity not found in collection '{collection}'" });
        }

        return TypedResults.Ok(entity.Value);
    }
}
