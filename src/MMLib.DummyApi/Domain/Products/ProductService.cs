using MMLib.DummyApi.Infrastructure;

namespace MMLib.DummyApi.Domain.Products;

public class ProductService
{
    private readonly DataStore<Guid, Product> _dataStore;

    public ProductService(DataStore<Guid, Product> dataStore)
    {
        _dataStore = dataStore;
    }

    public IEnumerable<Product> GetAll(string? category = null, int? minPrice = null)
    {
        var products = _dataStore.GetAll();

        if (!string.IsNullOrWhiteSpace(category))
            products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (minPrice.HasValue)
            products = products.Where(p => p.Price >= minPrice.Value);

        return products;
    }

    public Product? GetById(Guid id) => _dataStore.GetById(id);

    public (Product? product, List<string> errors) Create(CreateProductRequest request)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0)
            return (null, errors);

        var product = new Product(
            Id: Guid.NewGuid(),
            Name: request.Name,
            Description: request.Description ?? string.Empty,
            Price: request.Price,
            StockQuantity: request.StockQuantity,
            Category: request.Category,
            CreatedAt: DateTime.UtcNow
        );

        _dataStore.Add(product);
        return (product, errors);
    }

    public (Product? product, List<string> errors) Update(Guid id, UpdateProductRequest request)
    {
        var existing = _dataStore.GetById(id);
        if (existing == null)
            return (null, new List<string> { "Product not found" });

        var errors = ValidateUpdateRequest(request);
        if (errors.Count > 0)
            return (null, errors);

        var updated = existing with
        {
            Name = request.Name ?? existing.Name,
            Description = request.Description ?? existing.Description,
            Price = request.Price ?? existing.Price,
            StockQuantity = request.StockQuantity ?? existing.StockQuantity,
            Category = request.Category ?? existing.Category
        };

        _dataStore.Update(id, updated);
        return (updated, errors);
    }

    public bool Delete(Guid id) => _dataStore.Delete(id);

    private List<string> ValidateCreateRequest(CreateProductRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is required");

        if (request.Price <= 0)
            errors.Add("Price must be greater than 0");

        if (request.StockQuantity < 0)
            errors.Add("StockQuantity cannot be negative");

        return errors;
    }

    private List<string> ValidateUpdateRequest(UpdateProductRequest request)
    {
        var errors = new List<string>();

        if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name cannot be empty");

        if (request.Price.HasValue && request.Price <= 0)
            errors.Add("Price must be greater than 0");

        if (request.StockQuantity.HasValue && request.StockQuantity < 0)
            errors.Add("StockQuantity cannot be negative");

        return errors;
    }
}

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string Category
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    int? StockQuantity,
    string? Category
);
