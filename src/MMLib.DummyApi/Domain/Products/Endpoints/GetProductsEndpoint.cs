using MMLib.DummyApi.Domain.Products;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductsEndpoint
{
    public static RouteHandlerBuilder MapGetProducts(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/products", Handle)
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .WithTags("Products")
            .Produces<IEnumerable<Product>>(StatusCodes.Status200OK);
    }

    private static IResult Handle(
        ProductService productService,
        string? category = null,
        int? minPrice = null)
    {
        var products = productService.GetAll(category, minPrice);
        return Results.Ok(products);
    }
}
