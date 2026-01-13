using MMLib.DummyApi.Domain.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class PutProductEndpoint
{
    public static RouteHandlerBuilder MapPutProduct(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/{id:guid}", Handle)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .Accepts<UpdateProductRequest>("application/json");
    }

    private static HttpResults.Results<Ok<Product>, NotFound<object>, BadRequest<object>> Handle(
        Guid id,
        UpdateProductRequest request,
        ProductService productService)
    {
        var (product, errors) = productService.Update(id, request);
        
        if (product == null)
        {
            if (errors.Count > 0)
                return TypedResults.BadRequest<object>(new { errors });
            return TypedResults.NotFound<object>(new { error = "Product not found" });
        }

        return TypedResults.Ok(product);
    }
}
