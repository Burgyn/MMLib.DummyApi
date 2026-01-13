using MMLib.DummyApi.Infrastructure;

namespace MMLib.DummyApi.Domain.Orders;

public class OrderService
{
    private readonly DataStore<Guid, Order> _dataStore;

    public OrderService(DataStore<Guid, Order> dataStore)
    {
        _dataStore = dataStore;
    }

    public IEnumerable<Order> GetAll(string userId)
    {
        return _dataStore.GetAll().Where(o => o.UserId == userId);
    }

    public Order? GetById(Guid id, string userId)
    {
        var order = _dataStore.GetById(id);
        if (order == null || order.UserId != userId)
            return null;
        return order;
    }

    public Order? GetByIdForSystem(Guid id)
    {
        return _dataStore.GetById(id);
    }

    public (Order? order, List<string> errors) Create(CreateOrderRequest request, string userId)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0)
            return (null, errors);

        var order = new Order(
            Id: Guid.NewGuid(),
            UserId: userId,
            ProductIds: request.ProductIds,
            TotalAmount: request.TotalAmount,
            Status: OrderStatus.Pending,
            CreatedAt: DateTime.UtcNow
        );

        _dataStore.Add(order);
        return (order, errors);
    }

    public (Order? order, List<string> errors) Update(Guid id, UpdateOrderRequest request, string userId)
    {
        var existing = GetById(id, userId);
        if (existing == null)
            return (null, new List<string> { "Order not found" });

        var errors = ValidateUpdateRequest(request);
        if (errors.Count > 0)
            return (null, errors);

        var updated = existing with
        {
            ProductIds = request.ProductIds ?? existing.ProductIds,
            TotalAmount = request.TotalAmount ?? existing.TotalAmount
        };

        _dataStore.Update(id, updated);
        return (updated, errors);
    }

    public bool Delete(Guid id, string userId)
    {
        var order = GetById(id, userId);
        if (order == null)
            return false;

        return _dataStore.Delete(id);
    }

    public void UpdateStatus(Guid id, OrderStatus status)
    {
        var order = _dataStore.GetById(id);
        if (order == null)
            return;

        var updated = order with
        {
            Status = status,
            ProcessedAt = status == OrderStatus.Completed ? DateTime.UtcNow : order.ProcessedAt
        };

        _dataStore.Update(id, updated);
    }

    private List<string> ValidateCreateRequest(CreateOrderRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.UserId))
            errors.Add("UserId is required");

        if (request.ProductIds == null || request.ProductIds.Count == 0)
            errors.Add("ProductIds cannot be empty");

        if (request.TotalAmount <= 0)
            errors.Add("TotalAmount must be greater than 0");

        return errors;
    }

    private List<string> ValidateUpdateRequest(UpdateOrderRequest request)
    {
        var errors = new List<string>();

        if (request.ProductIds != null && request.ProductIds.Count == 0)
            errors.Add("ProductIds cannot be empty");

        if (request.TotalAmount.HasValue && request.TotalAmount <= 0)
            errors.Add("TotalAmount must be greater than 0");

        return errors;
    }
}

public record CreateOrderRequest(
    string UserId,
    List<Guid> ProductIds,
    decimal TotalAmount
);

public record UpdateOrderRequest(
    List<Guid>? ProductIds,
    decimal? TotalAmount
);
