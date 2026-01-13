using MMLib.DummyApi.Domain.Orders;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class PutOrderEndpoint
{
    public static RouteHandlerBuilder MapPutOrder(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/{id:guid}", Handle)
            .WithName("UpdateOrder")
            .WithSummary("Update an existing order")
            .Accepts<UpdateOrderRequest>("application/json");
    }

    private static HttpResults.Results<Ok<Order>, NotFound<object>, BadRequest<object>> Handle(
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
                return TypedResults.BadRequest<object>(new { errors });
            return TypedResults.NotFound<object>(new { error = "Order not found" });
        }

        return TypedResults.Ok(order);
    }
}
