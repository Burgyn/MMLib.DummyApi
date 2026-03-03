using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

/// <summary>
/// Endpoint for incrementing the performance counter.
/// </summary>
public static class IncrementCounterEndpoint
{
    /// <summary>
    /// Maps the POST /counter/increment endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapIncrementCounter(this IEndpointRouteBuilder app)
        => app.MapPost("/counter/increment", Handle)
            .WithName("IncrementCounter")
            .WithSummary("Increment the counter");

    private static Ok<CounterResponse> Handle(PerformanceCounter counter)
        => TypedResults.Ok(new CounterResponse { Value = counter.Increment() });
}
