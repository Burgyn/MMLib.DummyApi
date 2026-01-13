using MMLib.DummyApi.Features.System.Endpoints;

namespace MMLib.DummyApi.Features.System;

public static class Setup
{
    public static IServiceCollection AddSystem(this IServiceCollection services)
    {
        // No services needed for system endpoints
        return services;
    }

    public static IEndpointRouteBuilder MapSystem(this IEndpointRouteBuilder app)
    {
        ResetEndpoint.MapReset(app);
        HealthEndpoint.MapHealth(app);
        
        return app;
    }
}
