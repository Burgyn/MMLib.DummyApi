using AutoBogus;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Domain.Orders.Endpoints;
using MMLib.DummyApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Domain.Orders;

public static class Setup
{
    public static IServiceCollection AddOrders(this IServiceCollection services)
    {
        services.AddSingleton<DataStore<Guid, Order>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DummyApiOptions>>().Value;
            var dataStore = new DataStore<Guid, Order>(o => o.Id);
            
            // Seed initial data
            var faker = new AutoFaker<Order>()
                .RuleFor(o => o.Id, _ => Guid.NewGuid())
                .RuleFor(o => o.UserId, f => $"user-{f.Random.Int(1, 5)}")
                .RuleFor(o => o.ProductIds, f => Enumerable.Range(0, f.Random.Int(1, 5))
                    .Select(_ => Guid.NewGuid())
                    .ToList())
                .RuleFor(o => o.TotalAmount, f => f.Random.Decimal(10, 1000))
                .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
                .RuleFor(o => o.CreatedAt, _ => DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30)))
                .RuleFor(o => o.ProcessedAt, (f, o) => o.Status == OrderStatus.Completed 
                    ? DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 10)) 
                    : null);
            
            var orders = faker.Generate(options.InitialOrderCount);
            
            dataStore.Seed(orders);
            return dataStore;
        });

        services.AddScoped<OrderService>();
        
        return services;
    }

    public static IEndpointRouteBuilder MapOrders(this IEndpointRouteBuilder app)
    {
        GetOrdersEndpoint.MapGetOrders(app);
        GetOrderEndpoint.MapGetOrder(app);
        PostOrderEndpoint.MapPostOrder(app);
        PutOrderEndpoint.MapPutOrder(app);
        DeleteOrderEndpoint.MapDeleteOrder(app);
        GetOrderStatusEndpoint.MapGetOrderStatus(app);
        
        return app;
    }
}
