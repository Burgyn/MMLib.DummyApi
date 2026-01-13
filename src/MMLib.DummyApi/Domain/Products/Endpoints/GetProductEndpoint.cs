using MMLib.DummyApi.Domain.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class GetProductEndpoint
{
    public static RouteHandlerBuilder MapGetProduct(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{id:guid}", Handle)
            .WithName("GetProduct")
            .WithSummary("Get product by ID");
    }

    private static HttpResults.Results<Ok<Product>, NotFound<object>> Handle(Guid id, ProductService productService)
    {
        var product = productService.GetById(id);
        if (product == null)
            return TypedResults.NotFound<object>(new { error = "Product not found" });

        return TypedResults.Ok(product);
    }
}
