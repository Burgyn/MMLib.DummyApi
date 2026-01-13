using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrdersEndpoint
{
    public static RouteHandlerBuilder MapGetOrders(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/orders", Handle)
            .WithName("GetOrders")
            .WithSummary("Get all orders for the authenticated user")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<IEnumerable<Order>>(StatusCodes.Status200OK);
    }

    private static IResult Handle(OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var orders = orderService.GetAll(userId);
        return Results.Ok(orders);
    }
}
