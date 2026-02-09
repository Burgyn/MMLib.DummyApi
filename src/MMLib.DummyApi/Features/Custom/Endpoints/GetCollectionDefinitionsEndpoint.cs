using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCollectionDefinitionsEndpoint
{
    public static RouteHandlerBuilder MapGetCollectionDefinitions(this IEndpointRouteBuilder app)
        => app.MapGet("/_definitions", Handle)
            .WithName("GetCollectionDefinitions")
            .WithSummary("Get all collection definitions");

    private static Ok<IEnumerable<CollectionDefinition>> Handle(CustomDataStore dataStore)
    {
        var definitions = dataStore.GetAllDefinitions();
        return TypedResults.Ok(definitions);
    }
}
