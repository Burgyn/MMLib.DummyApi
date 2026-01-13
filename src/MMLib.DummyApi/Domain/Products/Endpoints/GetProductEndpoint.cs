using MMLib.DummyApi.Domain.Products;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductEndpoint
{
    public static RouteHandlerBuilder MapGetProduct(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/products/{id:guid}", Handle)
            .WithName("GetProduct")
            .WithSummary("Get product by ID")
            .WithTags("Products")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Handle(Guid id, ProductService productService)
    {
        var product = productService.GetById(id);
        if (product == null)
            return Results.NotFound(new { error = "Product not found" });

        return Results.Ok(product);
    }
}
