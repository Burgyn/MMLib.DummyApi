using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom;

namespace MMLib.DummyApi.Features.System.Endpoints;

/// <summary>
/// Endpoint for resetting all data or a specific collection.
/// </summary>
public static class ResetEndpoint
{
    /// <summary>
    /// Maps the POST /reset endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapReset(this IEndpointRouteBuilder app)
        => app.MapPost("/reset", Handle)
            .WithName("ResetData")
            .WithSummary("Reset all data or specific collection");

    private static Results<Ok<ResetResponse>, BadRequest<object>, NotFound<object>> Handle(
        CustomDataStore dataStore,
        AutoBogusSeeder seeder,
        string? collection = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            foreach (var name in dataStore.GetCollectionNames().ToList())
            {
                dataStore.ResetCollection(name);

                var definition = dataStore.GetDefinition(name);
                if (definition?.SeedCount > 0)
                {
                    var items = seeder.Generate(definition.Schema, definition.SeedCount);
                    foreach (var item in items)
                    {
                        dataStore.Add(name, item);
                    }
                }
            }

            return TypedResults.Ok(new ResetResponse { Message = "All collections reset successfully" });
        }

        if (!dataStore.CollectionExists(collection))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{collection}' not found" });
        }

        dataStore.ResetCollection(collection);

        var def = dataStore.GetDefinition(collection);
        if (def?.SeedCount > 0)
        {
            var items = seeder.Generate(def.Schema, def.SeedCount);
            foreach (var item in items)
            {
                dataStore.Add(collection, item);
            }
        }

        return TypedResults.Ok(new ResetResponse { Message = $"Collection '{collection}' reset successfully" });
    }
}

/// <summary>
/// Response model for the reset endpoint.
/// </summary>
public record ResetResponse
{
    /// <summary>
    /// A human-readable message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
