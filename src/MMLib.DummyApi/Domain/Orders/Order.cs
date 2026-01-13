namespace MMLib.DummyApi.Domain.Orders;

public record Order(
    Guid Id,
    string UserId,
    List<Guid> ProductIds,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt = null
);

public enum OrderStatus
{
    Pending,
    Processing,
    Completed
}
