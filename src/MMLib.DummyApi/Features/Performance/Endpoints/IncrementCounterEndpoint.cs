using MMLib.DummyApi.Features.Performance;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class IncrementCounterEndpoint
{
    public static RouteHandlerBuilder MapIncrementCounter(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/perf/counter/increment", Handle)
            .WithName("IncrementCounter")
            .WithSummary("Increment the counter")
            .WithTags("Performance")
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static IResult Handle(PerformanceCounter counter)
    {
        var newValue = counter.Increment();
        return Results.Ok(new { value = newValue });
    }
}
