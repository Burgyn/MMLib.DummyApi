using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCollectionDefinitionEndpoint
{
    public static RouteHandlerBuilder MapGetCollectionDefinition(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/_definitions/{name}", Handle)
            .WithName("GetCollectionDefinition")
            .WithSummary("Get a specific collection definition");
    }

    private static HttpResults.Results<Ok<CollectionDefinition>, NotFound<object>> Handle(
        string name,
        CustomDataStore dataStore)
    {
        var definition = dataStore.GetDefinition(name);
        if (definition == null)
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        return TypedResults.Ok(definition);
    }
}
