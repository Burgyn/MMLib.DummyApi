using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCustomCollectionEndpoint
{
    public static RouteHandlerBuilder MapGetCustomCollection(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{collection}", Handle)
            .WithName("GetCustomCollection")
            .WithSummary("Get all entities in a custom collection");
    }

    private static Ok<IEnumerable<JsonElement>> Handle(
        string collection,
        CustomCollectionService service)
    {
        var entities = service.GetAll(collection);
        return TypedResults.Ok(entities);
    }
}
