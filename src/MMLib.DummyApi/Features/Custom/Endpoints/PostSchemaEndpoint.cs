using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class PostSchemaEndpoint
{
    public static RouteHandlerBuilder MapPostSchema(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/{collection}/_schema", Handle)
            .WithName("SetSchema")
            .WithSummary("Define a JSON Schema for validation");
    }

    private static HttpResults.Results<Ok<SchemaResponse>, BadRequest<object>> Handle(
        string collection,
        JsonElement schema,
        CustomCollectionService service)
    {
        var (success, error) = service.SetSchema(collection, schema);

        if (!success)
        {
            return TypedResults.BadRequest<object>(new { error });
        }

        return TypedResults.Ok(new SchemaResponse
        {
            Collection = collection,
            Message = "Schema set successfully"
        });
    }
}

public record SchemaResponse
{
    public string Collection { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
