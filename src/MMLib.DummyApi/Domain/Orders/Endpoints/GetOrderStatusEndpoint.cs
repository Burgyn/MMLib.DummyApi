using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Infrastructure;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Domain.Orders.Endpoints;

public static class GetOrderStatusEndpoint
{
    public static RouteHandlerBuilder MapGetOrderStatus(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{id:guid}/status", Handle)
            .WithName("GetOrderStatus")
            .WithSummary("Get order background job status");
    }

    private static HttpResults.Results<Ok<OrderStatusResponse>, NotFound<object>> Handle(
        Guid id,
        OrderService orderService,
        BackgroundJobService backgroundJobService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "api-user";
        var order = orderService.GetById(id, userId);
        
        if (order == null)
            return TypedResults.NotFound<object>(new { error = "Order not found" });

        var status = backgroundJobService.GetOrderStatus(id);
        
        return TypedResults.Ok(new OrderStatusResponse
        {
            OrderId = id,
            Status = order.Status.ToString().ToLowerInvariant(),
            BackgroundJobStatus = status
        });
    }
}

public record OrderStatusResponse
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string BackgroundJobStatus { get; init; } = string.Empty;
}
