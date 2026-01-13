using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrdersEndpoint
{
    public static RouteHandlerBuilder MapGetOrders(this IEndpointRouteBuilder app)
    {
        return app.MapGet("", Handle)
            .WithName("GetOrders")
            .WithSummary("Get all orders for the authenticated user");
    }

    private static Ok<IEnumerable<Order>> Handle(OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var orders = orderService.GetAll(userId);
        return TypedResults.Ok(orders);
    }
}
