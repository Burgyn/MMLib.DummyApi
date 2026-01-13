using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.System.Endpoints;

public static class HealthEndpoint
{
    public static RouteHandlerBuilder MapHealth(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/health", Handle)
            .WithName("HealthCheck")
            .WithSummary("Health check endpoint");
    }

    private static Ok<HealthResponse> Handle()
    {
        return TypedResults.Ok(new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}

public record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
