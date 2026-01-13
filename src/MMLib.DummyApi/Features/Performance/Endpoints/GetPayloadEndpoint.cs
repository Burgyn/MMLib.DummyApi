using MMLib.DummyApi.Configuration;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Features.Performance.Endpoints;

public static class GetPayloadEndpoint
{
    public static RouteHandlerBuilder MapGetPayload(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/perf/payload", Handle)
            .WithName("GetPayload")
            .WithSummary("Generate payload of specified size")
            .WithTags("Performance")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(
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
                return Results.BadRequest(new { error = "Invalid size. Use: 1kb, 10kb, 100kb, 1mb" });
            }

            if (targetBytes > maxSizeMb * 1024 * 1024)
            {
                return Results.BadRequest(new { error = $"Size exceeds maximum of {maxSizeMb}MB" });
            }

            // Generate payload
            var payload = GeneratePayload(targetBytes);
            return Results.Ok(payload);
        }

        if (items.HasValue)
        {
            if (items.Value <= 0)
            {
                return Results.BadRequest(new { error = "Items must be greater than 0" });
            }

            var payload = GenerateItemsPayload(items.Value);
            return Results.Ok(payload);
        }

        return Results.BadRequest(new { error = "Specify either 'size' or 'items' parameter" });
    }

    private static object GeneratePayload(int targetBytes)
    {
        var item = new { id = Guid.NewGuid(), data = new string('x', Math.Max(1, targetBytes / 10)) };
        return new { size = targetBytes, item };
    }

    private static object GenerateItemsPayload(int itemCount)
    {
        var items = Enumerable.Range(1, itemCount)
            .Select(i => new
            {
                id = Guid.NewGuid(),
                name = $"Item {i}",
                value = i * 10,
                timestamp = DateTime.UtcNow
            })
            .ToList();

        return new { count = items.Count, items };
    }
}
