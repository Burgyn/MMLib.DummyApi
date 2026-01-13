using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrderEndpoint
{
    public static RouteHandlerBuilder MapGetOrder(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{id:guid}", Handle)
            .WithName("GetOrder")
            .WithSummary("Get order by ID");
    }

    private static HttpResults.Results<Ok<Order>, NotFound<object>> Handle(Guid id, OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var order = orderService.GetById(id, userId);
        
        if (order == null)
            return TypedResults.NotFound<object>(new { error = "Order not found" });

        return TypedResults.Ok(order);
    }
}
