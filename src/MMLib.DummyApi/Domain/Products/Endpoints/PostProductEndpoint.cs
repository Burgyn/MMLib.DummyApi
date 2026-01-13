using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class PostProductEndpoint
{
    public static RouteHandlerBuilder MapPostProduct(this IEndpointRouteBuilder app)
    {
        return app.MapPost("", Handle)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .Accepts<CreateProductRequest>("application/json");
    }

    private static HttpResults.Results<Created<Product>, BadRequest<object>> Handle(
        CreateProductRequest request,
        ProductService productService,
        BackgroundJobService backgroundJobService,
        HttpContext httpContext)
    {
        var (product, errors) = productService.Create(request);
        
        if (product == null)
            return TypedResults.BadRequest<object>(new { errors });

        // Schedule background job for calculated price
        var delayMs = GetBackgroundDelay(httpContext);
        backgroundJobService.ScheduleProductCalculation(product.Id, delayMs);

        return TypedResults.Created($"/products/{product.Id}", product);
    }

    private static int GetBackgroundDelay(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Background-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delay))
        {
            return delay;
        }

        // Default delay from configuration
        var options = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<MMLib.DummyApi.Configuration.DummyApiOptions>>();
        return options.Value.BackgroundJobDelayMs;
    }
}
