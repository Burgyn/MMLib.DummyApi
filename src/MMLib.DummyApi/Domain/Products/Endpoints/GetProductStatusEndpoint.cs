using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Infrastructure;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductStatusEndpoint
{
    public static RouteHandlerBuilder MapGetProductStatus(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/products/{id:guid}/status", Handle)
            .WithName("GetProductStatus")
            .WithSummary("Get product background job status")
            .WithTags("Products")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Handle(
        Guid id,
        ProductService productService,
        BackgroundJobService backgroundJobService)
    {
        var product = productService.GetById(id);
        if (product == null)
            return Results.NotFound(new { error = "Product not found" });

        var status = backgroundJobService.GetProductStatus(id);
        
        return Results.Ok(new
        {
            productId = id,
            calculatedPrice = product.CalculatedPrice,
            status = status
        });
    }
}
