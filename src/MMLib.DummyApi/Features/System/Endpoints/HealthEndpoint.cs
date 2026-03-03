using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.System.Endpoints;

/// <summary>
/// Endpoint for checking the health of the API.
/// </summary>
public static class HealthEndpoint
{
    /// <summary>
    /// Maps the GET /health endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapHealth(this IEndpointRouteBuilder app)
        => app.MapGet("/health", Handle)
            .WithName("HealthCheck")
            .WithSummary("Health check endpoint");

    private static Ok<HealthResponse> Handle()
        => TypedResults.Ok(new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow
        });
}

/// <summary>
/// Response model for the health check endpoint.
/// </summary>
public record HealthResponse
{
    /// <summary>
    /// The current health status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The UTC timestamp of the health check response.
    /// </summary>
    public DateTime Timestamp { get; init; }
}
