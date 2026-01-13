using MMLib.DummyApi.Features.Performance;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class GetCounterEndpoint
{
    public static RouteHandlerBuilder MapGetCounter(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/perf/counter", Handle)
            .WithName("GetCounter")
            .WithSummary("Get current counter value")
            .WithTags("Performance")
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static IResult Handle(PerformanceCounter counter)
    {
        return Results.Ok(new { value = counter.Get() });
    }
}
