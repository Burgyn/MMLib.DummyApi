using MMLib.DummyApi.Features.Performance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class GetCounterEndpoint
{
    public static RouteHandlerBuilder MapGetCounter(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/counter", Handle)
            .WithName("GetCounter")
            .WithSummary("Get current counter value");
    }

    private static Ok<CounterResponse> Handle(PerformanceCounter counter)
    {
        return TypedResults.Ok(new CounterResponse { Value = counter.Get() });
    }
}

public record CounterResponse
{
    public long Value { get; init; }
}
