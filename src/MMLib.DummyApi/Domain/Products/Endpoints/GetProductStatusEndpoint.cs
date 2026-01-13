using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductStatusEndpoint
{
    public static RouteHandlerBuilder MapGetProductStatus(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{id:guid}/status", Handle)
            .WithName("GetProductStatus")
            .WithSummary("Get product background job status");
    }

    private static HttpResults.Results<Ok<ProductStatusResponse>, NotFound<object>> Handle(
        Guid id,
        ProductService productService,
        BackgroundJobService backgroundJobService)
    {
        var product = productService.GetById(id);
        if (product == null)
            return TypedResults.NotFound<object>(new { error = "Product not found" });

        var status = backgroundJobService.GetProductStatus(id);
        
        return TypedResults.Ok(new ProductStatusResponse
        {
            ProductId = id,
            CalculatedPrice = product.CalculatedPrice,
            Status = status
        });
    }
}

public record ProductStatusResponse
{
    public Guid ProductId { get; init; }
    public decimal? CalculatedPrice { get; init; }
    public string Status { get; init; } = string.Empty;
}
