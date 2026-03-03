using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

/// <summary>
/// Endpoint for resetting the performance counter to zero.
/// </summary>
public static class ResetCounterEndpoint
{
    /// <summary>
    /// Maps the POST /counter/reset endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapResetCounter(this IEndpointRouteBuilder app)
        => app.MapPost("/counter/reset", Handle)
            .WithName("ResetCounter")
            .WithSummary("Reset the counter to zero");

    private static NoContent Handle(PerformanceCounter counter)
    {
        counter.Reset();
        return TypedResults.NoContent();
    }
}
