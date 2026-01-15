using MMLib.DummyApi.Features.Custom;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.System.Endpoints;

public static class ResetEndpoint
{
    public static RouteHandlerBuilder MapReset(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/reset", Handle)
            .WithName("ResetData")
            .WithSummary("Reset all data or specific collection");
    }

    private static HttpResults.Results<Ok<ResetResponse>, BadRequest<object>, NotFound<object>> Handle(
        CustomDataStore dataStore,
        AutoBogusSeeder seeder,
        string? collection = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            // Reset all collections - clear data and re-seed
            foreach (var name in dataStore.GetCollectionNames().ToList())
            {
                dataStore.ResetCollection(name);
                
                // Re-seed data if seedCount > 0
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

        // Reset specific collection
        if (!dataStore.CollectionExists(collection))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{collection}' not found" });
        }

        dataStore.ResetCollection(collection);
        
        // Re-seed if needed
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

public record ResetResponse
{
    public string Message { get; init; } = string.Empty;
}
