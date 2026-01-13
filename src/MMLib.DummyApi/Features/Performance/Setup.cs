using MMLib.DummyApi.Features.Performance.Endpoints;

namespace MMLib.DummyApi.Features.Performance;

public static class Setup
{
    public static IServiceCollection AddPerformance(this IServiceCollection services)
    {
        services.AddSingleton<PerformanceCounter>();
        return services;
    }

    public static IEndpointRouteBuilder MapPerformance(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/perf")
            .WithTags("Performance");

        group.MapGetPayload();
        group.MapGetCounter();
        group.MapIncrementCounter();
        group.MapResetCounter();
        
        return app;
    }
}
