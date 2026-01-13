using MMLib.DummyApi.Domain.Products;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class PutProductEndpoint
{
    public static RouteHandlerBuilder MapPutProduct(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/products/{id:guid}", Handle)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .WithTags("Products")
            .Accepts<UpdateProductRequest>("application/json")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(
        Guid id,
        UpdateProductRequest request,
        ProductService productService)
    {
        var (product, errors) = productService.Update(id, request);
        
        if (product == null)
        {
            if (errors.Count > 0)
                return Results.BadRequest(new { errors });
            return Results.NotFound(new { error = "Product not found" });
        }

        return Results.Ok(product);
    }
}
