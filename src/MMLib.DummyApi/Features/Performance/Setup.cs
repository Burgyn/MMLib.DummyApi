using MMLib.DummyApi.Features.Performance.Endpoints;

namespace MMLib.DummyApi.Features.Performance;

/// <summary>
/// Registration and mapping helpers for the Performance feature.
/// </summary>
public static class Setup
{
    /// <summary>
    /// Registers Performance feature services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddPerformance(this IServiceCollection services)
    {
        services.AddSingleton<PerformanceCounter>();
        return services;
    }

    /// <summary>
    /// Maps all Performance feature endpoints under /perf.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
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
