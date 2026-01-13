using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Infrastructure;
using System.Security.Claims;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrderStatusEndpoint
{
    public static RouteHandlerBuilder MapGetOrderStatus(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/orders/{id:guid}/status", Handle)
            .WithName("GetOrderStatus")
            .WithSummary("Get order background job status")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Handle(
        Guid id,
        OrderService orderService,
        BackgroundJobService backgroundJobService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var order = orderService.GetById(id, userId);
        
        if (order == null)
            return Results.NotFound(new { error = "Order not found" });

        var status = backgroundJobService.GetOrderStatus(id);
        
        return Results.Ok(new
        {
            orderId = id,
            status = order.Status.ToString().ToLowerInvariant(),
            backgroundJobStatus = status
        });
    }
}
