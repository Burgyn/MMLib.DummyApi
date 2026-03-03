using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

/// <summary>
/// Endpoint for retrieving the current counter value.
/// </summary>
public static class GetCounterEndpoint
{
    /// <summary>
    /// Maps the GET /counter endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCounter(this IEndpointRouteBuilder app)
        => app.MapGet("/counter", Handle)
            .WithName("GetCounter")
            .WithSummary("Get current counter value");

    private static Ok<CounterResponse> Handle(PerformanceCounter counter)
        => TypedResults.Ok(new CounterResponse { Value = counter.Get() });
}

/// <summary>
/// Response model for counter endpoints.
/// </summary>
public record CounterResponse
{
    /// <summary>
    /// The current counter value.
    /// </summary>
    public long Value { get; init; }
}
