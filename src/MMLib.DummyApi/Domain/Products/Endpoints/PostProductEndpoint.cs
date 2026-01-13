using MMLib.DummyApi.Domain.Products;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Domain.Products.Endpoints;

public static class PostProductEndpoint
{
    public static RouteHandlerBuilder MapPostProduct(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/products", Handle)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithTags("Products")
            .Accepts<CreateProductRequest>("application/json")
            .Produces<Product>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(
        CreateProductRequest request,
        ProductService productService,
        BackgroundJobService backgroundJobService,
        HttpContext httpContext)
    {
        var (product, errors) = productService.Create(request);
        
        if (product == null)
            return Results.BadRequest(new { errors });

        // Schedule background job for calculated price
        var delayMs = GetBackgroundDelay(httpContext);
        backgroundJobService.ScheduleProductCalculation(product.Id, delayMs);

        return Results.Created($"/products/{product.Id}", product);
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
