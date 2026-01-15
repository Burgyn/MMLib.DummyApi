using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom.Models;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class PostBackgroundConfigEndpoint
{
    public static RouteHandlerBuilder MapPostBackgroundConfig(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/{collection}/_background", Handle)
            .WithName("SetBackgroundConfig")
            .WithSummary("Configure a background job for a collection");
    }

    private static HttpResults.Results<Ok<BackgroundConfigResponse>, BadRequest<object>> Handle(
        string collection,
        BackgroundJobConfig config,
        CustomCollectionService service)
    {
        if (string.IsNullOrWhiteSpace(config.FieldPath))
        {
            return TypedResults.BadRequest<object>(new { error = "FieldPath is required" });
        }

        if (string.IsNullOrWhiteSpace(config.Operation))
        {
            return TypedResults.BadRequest<object>(new { error = "Operation is required" });
        }

        // Validate operation format
        var validOperations = new[] { "sequence:", "sum:", "count:", "timestamp", "random:" };
        if (!validOperations.Any(op => config.Operation.StartsWith(op) || config.Operation == "timestamp"))
        {
            return TypedResults.BadRequest<object>(new 
            { 
                error = "Invalid operation. Supported: sequence:val1,val2,... | sum:path | count:path | timestamp | random:min,max" 
            });
        }

        service.SetBackgroundConfig(collection, config);

        return TypedResults.Ok(new BackgroundConfigResponse
        {
            Collection = collection,
            Config = config,
            Message = "Background job configured successfully"
        });
    }
}

public record BackgroundConfigResponse
{
    public string Collection { get; init; } = string.Empty;
    public BackgroundJobConfig Config { get; init; } = null!;
    public string Message { get; init; } = string.Empty;
}
