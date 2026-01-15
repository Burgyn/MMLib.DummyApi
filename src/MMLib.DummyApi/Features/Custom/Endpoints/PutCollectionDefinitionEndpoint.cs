using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class PutCollectionDefinitionEndpoint
{
    public static RouteHandlerBuilder MapPutCollectionDefinition(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/_definitions/{name}", Handle)
            .WithName("UpdateCollectionDefinition")
            .WithSummary("Update a collection definition (does not affect existing data)");
    }

    private static HttpResults.Results<Ok<CollectionDefinition>, NotFound<object>, BadRequest<object>> Handle(
        string name,
        CollectionDefinition definition,
        CustomDataStore dataStore)
    {
        if (!dataStore.CollectionExists(name))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        // Ensure name matches
        var updatedDefinition = definition with { Name = name };
        dataStore.SaveDefinition(updatedDefinition);

        return TypedResults.Ok(updatedDefinition);
    }
}
