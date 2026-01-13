using MMLib.DummyApi.Features.Performance;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class ResetCounterEndpoint
{
    public static RouteHandlerBuilder MapResetCounter(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/perf/counter/reset", Handle)
            .WithName("ResetCounter")
            .WithSummary("Reset the counter to zero")
            .WithTags("Performance")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static IResult Handle(PerformanceCounter counter)
    {
        counter.Reset();
        return Results.NoContent();
    }
}
