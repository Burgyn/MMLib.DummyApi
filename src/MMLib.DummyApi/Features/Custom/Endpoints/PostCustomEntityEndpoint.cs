using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Infrastructure;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class PostCustomEntityEndpoint
{
    public static RouteHandlerBuilder MapPostCustomEntity(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/{collection}", Handle)
            .WithName("CreateCustomEntity")
            .WithSummary("Create a new entity in a custom collection");
    }

    private static HttpResults.Results<Created<JsonElement>, BadRequest<object>> Handle(
        string collection,
        JsonElement data,
        CustomCollectionService service,
        BackgroundJobService backgroundJobService,
        CustomDataStore dataStore,
        HttpContext httpContext)
    {
        var (entity, errors) = service.Create(collection, data);

        if (entity == null)
        {
            return TypedResults.BadRequest<object>(new { errors });
        }

        // Get id from the created entity
        var id = Guid.Parse(entity.Value.GetProperty("id").GetString()!);

        // Schedule background job if configured
        var config = dataStore.GetBackgroundConfig(collection);
        if (config != null)
        {
            var delayMs = GetBackgroundDelay(httpContext, config.DelayMs);
            backgroundJobService.ScheduleCustomJob(collection, id, delayMs);
        }

        return TypedResults.Created($"/custom/{collection}/{id}", entity.Value);
    }

    private static int GetBackgroundDelay(HttpContext httpContext, int defaultDelay)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Background-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delay))
        {
            return delay;
        }
        return defaultDelay;
    }
}
