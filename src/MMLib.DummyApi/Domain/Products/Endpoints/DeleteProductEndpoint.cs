using MMLib.DummyApi.Domain.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class DeleteProductEndpoint
{
    public static RouteHandlerBuilder MapDeleteProduct(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{id:guid}", Handle)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(Guid id, ProductService productService)
    {
        var deleted = productService.Delete(id);
        if (!deleted)
            return TypedResults.NotFound<object>(new { error = "Product not found" });

        return TypedResults.NoContent();
    }
}
