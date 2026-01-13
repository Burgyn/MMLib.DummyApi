using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class DeleteOrderEndpoint
{
    public static RouteHandlerBuilder MapDeleteOrder(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/orders/{id:guid}", Handle)
            .WithName("DeleteOrder")
            .WithSummary("Delete an order")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Handle(Guid id, OrderService orderService, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var deleted = orderService.Delete(id, userId);
        
        if (!deleted)
            return Results.NotFound(new { error = "Order not found" });

        return Results.NoContent();
    }
}
