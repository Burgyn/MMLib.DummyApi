using Microsoft.AspNetCore.Http.HttpResults;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Infrastructure;
using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for creating a new entity in a collection.
/// </summary>
public static class PostCustomEntityEndpoint
{
    /// <summary>
    /// Maps the POST /{collection} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapPostCustomEntity(this IEndpointRouteBuilder app)
        => app.MapPost("/{collection}", Handle)
            .WithName("CreateCustomEntity")
            .WithSummary("Create a new entity in a collection");

    private static Results<Created<JsonElement>, BadRequest<object>, NotFound<object>, UnauthorizedHttpResult> Handle(
        string collection,
        JsonElement data,
        CustomCollectionService service,
        BackgroundJobService backgroundJobService,
        HttpContext httpContext)
    {
        if (!service.CollectionExists(collection))
        {
            return TypedResults.NotFound<object>(new { error = $"Collection '{collection}' not found. Create it first via POST /custom/_definitions" });
        }

        if (service.IsAuthRequired(collection) && !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return TypedResults.Unauthorized();
        }

        var (entity, errors) = service.Create(collection, data);

        if (entity == null)
        {
            return TypedResults.BadRequest<object>(new { errors });
        }

        var id = Guid.Parse(entity.Value.GetProperty("id").GetString()!);

        var config = service.GetBackgroundConfig(collection);
        if (config != null)
        {
            var delayMs = DynamicEndpointMapper.GetBackgroundDelay(httpContext, config.DelayMs);
            backgroundJobService.ScheduleCustomJob(collection, id, delayMs);
        }

        return TypedResults.Created($"/custom/{collection}/{id}", entity.Value);
    }
}
