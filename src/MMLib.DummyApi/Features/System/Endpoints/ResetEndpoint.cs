using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.System.Endpoints;

public static class ResetEndpoint
{
    public static RouteHandlerBuilder MapReset(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/reset", Handle)
            .WithName("ResetData")
            .WithSummary("Reset all data to initial state");
    }

    private static HttpResults.Results<Ok<ResetResponse>, BadRequest<object>> Handle(
        IServiceProvider serviceProvider,
        string? entity = null)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            // Reset all entities
            var productDataStore = serviceProvider.GetRequiredService<DataStore<Guid, Product>>();
            var orderDataStore = serviceProvider.GetRequiredService<DataStore<Guid, Order>>();
            var customDataStore = serviceProvider.GetRequiredService<CustomDataStore>();
            
            productDataStore.Reset();
            orderDataStore.Reset();
            customDataStore.ResetAll();
            
            return TypedResults.Ok(new ResetResponse { Message = "All data reset successfully" });
        }

        // Reset specific entity
        entity = entity.ToLowerInvariant();
        switch (entity)
        {
            case "products":
                var productStore = serviceProvider.GetRequiredService<DataStore<Guid, Product>>();
                productStore.Reset();
                return TypedResults.Ok(new ResetResponse { Message = "Products reset successfully" });
            
            case "orders":
                var orderStore = serviceProvider.GetRequiredService<DataStore<Guid, Order>>();
                orderStore.Reset();
                return TypedResults.Ok(new ResetResponse { Message = "Orders reset successfully" });
            
            case "custom":
                var customStore = serviceProvider.GetRequiredService<CustomDataStore>();
                customStore.ResetAll();
                return TypedResults.Ok(new ResetResponse { Message = "Custom collections reset successfully" });
            
            default:
                return TypedResults.BadRequest<object>(new { error = $"Unknown entity: {entity}. Valid values: products, orders, custom" });
        }
    }
}

public record ResetResponse
{
    public string Message { get; init; } = string.Empty;
}
