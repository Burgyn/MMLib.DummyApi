using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using MMLib.DummyApi.Configuration;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class PostOrderEndpoint
{
    public static RouteHandlerBuilder MapPostOrder(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/orders", Handle)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .WithTags("Orders")
            .RequireAuthorization()
            .Accepts<CreateOrderRequest>("application/json")
            .Produces<Order>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(
        CreateOrderRequest request,
        OrderService orderService,
        BackgroundJobService backgroundJobService,
        ClaimsPrincipal user,
        HttpContext httpContext)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        
        // Override UserId from request with authenticated user
        var createRequest = request with { UserId = userId };
        var (order, errors) = orderService.Create(createRequest, userId);
        
        if (order == null)
            return Results.BadRequest(new { errors });

        // Schedule background job for status update
        var delayMs = GetBackgroundDelay(httpContext);
        backgroundJobService.ScheduleOrderStatusUpdate(order.Id, delayMs);

        return Results.Created($"/orders/{order.Id}", order);
    }

    private static int GetBackgroundDelay(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Background-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delay))
        {
            return delay;
        }

        var options = httpContext.RequestServices.GetRequiredService<IOptions<DummyApiOptions>>();
        return options.Value.BackgroundJobDelayMs;
    }
}
