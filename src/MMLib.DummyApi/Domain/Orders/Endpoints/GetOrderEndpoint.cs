using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrderEndpoint
{
    public static RouteHandlerBuilder MapGetOrder(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/orders/{id:guid}", Handle)
            .WithName("GetOrder")
            .WithSummary("Get order by ID")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Handle(Guid id, OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var order = orderService.GetById(id, userId);
        
        if (order == null)
            return Results.NotFound(new { error = "Order not found" });

        return Results.Ok(order);
    }
}
