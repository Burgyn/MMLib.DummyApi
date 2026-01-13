using MMLib.DummyApi.Domain.Products;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductsEndpoint
{
    public static RouteHandlerBuilder MapGetProducts(this IEndpointRouteBuilder app)
    {
        return app.MapGet("", Handle)
            .WithName("GetProducts")
            .WithSummary("Get all products");
    }

    private static Ok<IEnumerable<Product>> Handle(
        ProductService productService,
        string? category = null,
        int? minPrice = null)
    {
        var products = productService.GetAll(category, minPrice);
        return TypedResults.Ok(products);
    }
}
