namespace MMLib.DummyApi.Features.System.Endpoints;

public static class HealthEndpoint
{
    public static RouteHandlerBuilder MapHealth(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/health", Handle)
            .WithName("HealthCheck")
            .WithSummary("Health check endpoint")
            .WithTags("System")
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static IResult Handle()
    {
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
