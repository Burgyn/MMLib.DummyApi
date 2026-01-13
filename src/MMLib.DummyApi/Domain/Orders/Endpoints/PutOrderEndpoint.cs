using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class PutOrderEndpoint
{
    public static RouteHandlerBuilder MapPutOrder(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/orders/{id:guid}", Handle)
            .WithName("UpdateOrder")
            .WithSummary("Update an existing order")
            .WithTags("Orders")
            .RequireAuthorization()
            .Accepts<UpdateOrderRequest>("application/json")
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(
        Guid id,
        UpdateOrderRequest request,
        OrderService orderService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var (order, errors) = orderService.Update(id, request, userId);
        
        if (order == null)
        {
            if (errors.Count > 0)
                return Results.BadRequest(new { errors });
            return Results.NotFound(new { error = "Order not found" });
        }

        return Results.Ok(order);
    }
}
