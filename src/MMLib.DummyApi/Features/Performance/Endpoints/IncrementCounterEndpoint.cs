using MMLib.DummyApi.Features.Performance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class IncrementCounterEndpoint
{
    public static RouteHandlerBuilder MapIncrementCounter(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/counter/increment", Handle)
            .WithName("IncrementCounter")
            .WithSummary("Increment the counter");
    }

    private static Ok<CounterResponse> Handle(PerformanceCounter counter)
    {
        var newValue = counter.Increment();
        return TypedResults.Ok(new CounterResponse { Value = newValue });
    }
}
