using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Infrastructure;

namespace MMLib.DummyApi.Features.System.Endpoints;

public static class ResetEndpoint
{
    public static RouteHandlerBuilder MapReset(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/reset", Handle)
            .WithName("ResetData")
            .WithSummary("Reset all data to initial state")
            .WithTags("System")
            .Produces(StatusCodes.Status200OK);
    }

    private static IResult Handle(
        IServiceProvider serviceProvider,
        string? entity = null)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            // Reset all entities
            var productDataStore = serviceProvider.GetRequiredService<DataStore<Guid, Product>>();
            var orderDataStore = serviceProvider.GetRequiredService<DataStore<Guid, Order>>();
            
            productDataStore.Reset();
            orderDataStore.Reset();
            
            return Results.Ok(new { message = "All data reset successfully" });
        }

        // Reset specific entity
        entity = entity.ToLowerInvariant();
        switch (entity)
        {
            case "products":
                var productStore = serviceProvider.GetRequiredService<DataStore<Guid, Product>>();
                productStore.Reset();
                return Results.Ok(new { message = "Products reset successfully" });
            
            case "orders":
                var orderStore = serviceProvider.GetRequiredService<DataStore<Guid, Order>>();
                orderStore.Reset();
                return Results.Ok(new { message = "Orders reset successfully" });
            
            default:
                return Results.BadRequest(new { error = $"Unknown entity: {entity}" });
        }
    }
}
