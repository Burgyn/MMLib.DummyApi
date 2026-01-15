using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetSchemaEndpoint
{
    public static RouteHandlerBuilder MapGetSchema(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{collection}/_schema", Handle)
            .WithName("GetSchema")
            .WithSummary("Get the JSON Schema for a collection");
    }

    private static HttpResults.Results<Ok<JsonElement>, NotFound<object>> Handle(
        string collection,
        CustomCollectionService service)
    {
        var schema = service.GetSchema(collection);
        
        if (schema == null)
        {
            return TypedResults.NotFound<object>(new { error = $"No schema defined for collection '{collection}'" });
        }

        return TypedResults.Ok(schema.Value);
    }
}
