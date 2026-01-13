using MMLib.DummyApi.Features.Performance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class ResetCounterEndpoint
{
    public static RouteHandlerBuilder MapResetCounter(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/counter/reset", Handle)
            .WithName("ResetCounter")
            .WithSummary("Reset the counter to zero");
    }

    private static NoContent Handle(PerformanceCounter counter)
    {
        counter.Reset();
        return TypedResults.NoContent();
    }
}
