using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MMLib.DummyApi.Configuration;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

/// <summary>
/// Endpoint for generating payloads of a specified size or item count.
/// </summary>
public static class GetPayloadEndpoint
{
    /// <summary>
    /// Maps the GET /payload endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetPayload(this IEndpointRouteBuilder app)
        => app.MapGet("/payload", Handle)
            .WithName("GetPayload")
            .WithSummary("Generate payload of specified size");

    private static Results<Ok<SizePayloadResponse>, Ok<ItemsPayloadResponse>, BadRequest<object>> Handle(
        string? size = null,
        int? items = null,
        IOptions<DummyApiOptions>? options = null)
    {
        var maxSizeMb = options?.Value.Performance.MaxPayloadSizeMb ?? 10;

        if (!string.IsNullOrWhiteSpace(size))
        {
            var sizeLower = size.ToLowerInvariant();
            int targetBytes = sizeLower switch
            {
                "1kb" => 1024,
                "10kb" => 10 * 1024,
                "100kb" => 100 * 1024,
                "1mb" => 1024 * 1024,
                _ => 0
            };

            if (targetBytes == 0)
            {
                return TypedResults.BadRequest<object>(new { error = "Invalid size. Use: 1kb, 10kb, 100kb, 1mb" });
            }

            if (targetBytes > maxSizeMb * 1024 * 1024)
            {
                return TypedResults.BadRequest<object>(new { error = $"Size exceeds maximum of {maxSizeMb}MB" });
            }

            var payload = GeneratePayload(targetBytes);
            return TypedResults.Ok(payload);
        }

        if (items.HasValue)
        {
            if (items.Value <= 0)
            {
                return TypedResults.BadRequest<object>(new { error = "Items must be greater than 0" });
            }

            var payload = GenerateItemsPayload(items.Value);
            return TypedResults.Ok(payload);
        }

        return TypedResults.BadRequest<object>(new { error = "Specify either 'size' or 'items' parameter" });
    }

    private static SizePayloadResponse GeneratePayload(int targetBytes)
        => new SizePayloadResponse
        {
            Size = targetBytes,
            Item = new PayloadItem
            {
                Id = Guid.NewGuid(),
                Data = new string('x', Math.Max(1, targetBytes / 10))
            }
        };

    private static ItemsPayloadResponse GenerateItemsPayload(int itemCount)
    {
        List<PayloadItem> items = Enumerable.Range(1, itemCount)
            .Select(i => new PayloadItem
            {
                Id = Guid.NewGuid(),
                Name = $"Item {i}",
                Value = i * 10,
                Timestamp = DateTime.UtcNow
            })
            .ToList();

        return new ItemsPayloadResponse
        {
            Count = items.Count,
            Items = items
        };
    }
}

/// <summary>
/// Response model for size-based payload generation.
/// </summary>
public record SizePayloadResponse
{
    /// <summary>
    /// The requested payload size in bytes.
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// The generated payload item.
    /// </summary>
    public PayloadItem Item { get; init; } = null!;
}

/// <summary>
/// Response model for item-count-based payload generation.
/// </summary>
public record ItemsPayloadResponse
{
    /// <summary>
    /// The number of generated items.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// The generated items.
    /// </summary>
    public List<PayloadItem> Items { get; init; } = [];
}

/// <summary>
/// A single payload item used in performance tests.
/// </summary>
public record PayloadItem
{
    /// <summary>
    /// Unique identifier of the item.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Optional item name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional numeric value.
    /// </summary>
    public int? Value { get; init; }

    /// <summary>
    /// Optional timestamp.
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// Optional binary-like string data used for size-based payloads.
    /// </summary>
    public string? Data { get; init; }
}
