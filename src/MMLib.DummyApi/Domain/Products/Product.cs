namespace MMLib.DummyApi.Domain.Products;

public record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    DateTime CreatedAt,
    decimal? CalculatedPrice = null
);
