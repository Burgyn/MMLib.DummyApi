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

    private static IResult Handle(
        string collection,
        JsonElement data,
        CustomCollectionService service,
        BackgroundJobService backgroundJobService,
        RuleResolver ruleResolver,
        HttpContext httpContext)
    {
        if (!service.CollectionExists(collection))
        {
            return Results.NotFound(new { error = $"Collection '{collection}' not found. Create it first via POST /custom/_definitions" });
        }

        if (service.IsAuthRequired(collection) && !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return Results.Unauthorized();
        }

        List<Models.ResponseRule>? rules = service.GetRules(collection);
        Models.RuleResponse? ruleResponse = ruleResolver.TryMatchRule(rules, "POST", httpContext, data);
        if (ruleResponse != null)
        {
            return DynamicEndpointMapper.ApplyRuleResponse(ruleResponse, httpContext);
        }

        (JsonElement? entity, List<string> errors) = service.Create(collection, data);

        if (entity == null)
        {
            return Results.BadRequest(new { errors });
        }

        Guid id = Guid.Parse(entity.Value.GetProperty("id").GetString()!);

        Models.BackgroundJobConfig? config = service.GetBackgroundConfig(collection);
        if (config != null)
        {
            int delayMs = DynamicEndpointMapper.GetBackgroundDelay(httpContext, config.DelayMs);
            backgroundJobService.ScheduleCustomJob(collection, id, delayMs);
        }

        return Results.Created($"/custom/{collection}/{id}", entity.Value);
    }
}
