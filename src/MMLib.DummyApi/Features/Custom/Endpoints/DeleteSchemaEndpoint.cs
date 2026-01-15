using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class DeleteSchemaEndpoint
{
    public static RouteHandlerBuilder MapDeleteSchema(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{collection}/_schema", Handle)
            .WithName("DeleteSchema")
            .WithSummary("Delete the JSON Schema for a collection");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(
        string collection,
        CustomCollectionService service)
    {
        var deleted = service.DeleteSchema(collection);
        
        if (!deleted)
        {
            return TypedResults.NotFound<object>(new { error = $"No schema defined for collection '{collection}'" });
        }

        return TypedResults.NoContent();
    }
}
