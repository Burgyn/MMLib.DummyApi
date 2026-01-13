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
        GetPayloadEndpoint.MapGetPayload(app);
        GetCounterEndpoint.MapGetCounter(app);
        IncrementCounterEndpoint.MapIncrementCounter(app);
        ResetCounterEndpoint.MapResetCounter(app);
        
        return app;
    }
}
