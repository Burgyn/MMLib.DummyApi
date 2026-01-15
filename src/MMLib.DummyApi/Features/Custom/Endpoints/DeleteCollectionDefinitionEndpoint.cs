using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class DeleteCollectionDefinitionEndpoint
{
    public static RouteHandlerBuilder MapDeleteCollectionDefinition(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/_definitions/{name}", Handle)
            .WithName("DeleteCollectionDefinition")
            .WithSummary("Delete a collection and all its data");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(
        string name,
        CustomDataStore dataStore)
    {
        if (!dataStore.DeleteDefinition(name))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{name}' not found" });
        }

        return TypedResults.NoContent();
    }
}
