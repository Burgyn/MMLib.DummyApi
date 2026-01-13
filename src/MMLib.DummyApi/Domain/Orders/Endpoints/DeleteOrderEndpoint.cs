using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class DeleteOrderEndpoint
{
    public static RouteHandlerBuilder MapDeleteOrder(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{id:guid}", Handle)
            .WithName("DeleteOrder")
            .WithSummary("Delete an order");
    }

    private static HttpResults.Results<NoContent, NotFound<object>> Handle(Guid id, OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var deleted = orderService.Delete(id, userId);
        
        if (!deleted)
            return TypedResults.NotFound<object>(new { error = "Order not found" });

        return TypedResults.NoContent();
    }
}
