using AutoBogus;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Domain.Products.Endpoints;
using MMLib.DummyApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Domain.Products;

public static class Setup
{
    public static IServiceCollection AddProducts(this IServiceCollection services)
    {
        services.AddSingleton<DataStore<Guid, Product>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DummyApiOptions>>().Value;
            var dataStore = new DataStore<Guid, Product>(p => p.Id);
            
            // Seed initial data
            var faker = new AutoFaker<Product>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 100))
                .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
                .RuleFor(p => p.CreatedAt, _ => DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30)))
                .RuleFor(p => p.CalculatedPrice, _ => null);
            
            var products = faker.Generate(options.InitialProductCount);
            
            dataStore.Seed(products);
            return dataStore;
        });

        services.AddScoped<ProductService>();
        
        return services;
    }

    public static IEndpointRouteBuilder MapProducts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products");

        group.MapGetProducts();
        group.MapGetProduct();
        group.MapPostProduct();
        group.MapPutProduct();
        group.MapDeleteProduct();
        group.MapGetProductStatus();
        
        return app;
    }
}
