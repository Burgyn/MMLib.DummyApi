using MMLib.DummyApi.Features.System.Endpoints;

namespace MMLib.DummyApi.Features.System;

/// <summary>
/// Registration and mapping helpers for the System feature.
/// </summary>
public static class Setup
{
    /// <summary>
    /// Registers System feature services (currently none required).
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddSystem(this IServiceCollection services)
        => services;

    /// <summary>
    /// Maps all System feature endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapSystem(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("System");

        group.MapReset();
        group.MapHealth();

        return app;
    }
}
