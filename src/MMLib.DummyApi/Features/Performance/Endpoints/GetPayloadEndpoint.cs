using MMLib.DummyApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class GetPayloadEndpoint
{
    public static RouteHandlerBuilder MapGetPayload(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/payload", Handle)
            .WithName("GetPayload")
            .WithSummary("Generate payload of specified size");
    }

    private static HttpResults.Results<Ok<SizePayloadResponse>, Ok<ItemsPayloadResponse>, BadRequest<object>> Handle(
        string? size = null,
        int? items = null,
        IOptions<DummyApiOptions>? options = null)
    {
        var maxSizeMb = options?.Value.Performance.MaxPayloadSizeMb ?? 10;

        if (!string.IsNullOrWhiteSpace(size))
        {
            // Parse size (1kb, 10kb, 100kb, 1mb)
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

            // Generate payload
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
    {
        return new SizePayloadResponse
        {
            Size = targetBytes,
            Item = new PayloadItem
            {
                Id = Guid.NewGuid(),
                Data = new string('x', Math.Max(1, targetBytes / 10))
            }
        };
    }

    private static ItemsPayloadResponse GenerateItemsPayload(int itemCount)
    {
        var items = Enumerable.Range(1, itemCount)
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

public record SizePayloadResponse
{
    public int Size { get; init; }
    public PayloadItem Item { get; init; } = null!;
}

public record ItemsPayloadResponse
{
    public int Count { get; init; }
    public List<PayloadItem> Items { get; init; } = new();
}

public record PayloadItem
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public int? Value { get; init; }
    public DateTime? Timestamp { get; init; }
    public string? Data { get; init; }
}
