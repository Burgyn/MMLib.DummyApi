using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving all entities in a collection.
/// </summary>
public static class GetCustomCollectionEndpoint
{
    /// <summary>
    /// Maps the GET /{collection} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCustomCollection(this IEndpointRouteBuilder app)
        => app.MapGet("/{collection}", Handle)
            .WithName("GetCustomCollection")
            .WithSummary("Get all entities in a collection");

    private static IResult Handle(
        string collection,
        CustomCollectionService service,
        RuleResolver ruleResolver,
        HttpContext httpContext)
    {
        if (!service.CollectionExists(collection))
        {
            return Results.NotFound(new { error = $"Collection '{collection}' not found" });
        }

        if (service.IsAuthRequired(collection) && !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return Results.Unauthorized();
        }

        List<Models.ResponseRule>? rules = service.GetRules(collection);
        Models.RuleResponse? ruleResponse = ruleResolver.TryMatchRule(rules, "GET", httpContext);
        if (ruleResponse != null)
        {
            return DynamicEndpointMapper.ApplyRuleResponse(ruleResponse, httpContext);
        }

        IEnumerable<JsonElement> entities = service.GetAll(collection);
        return Results.Ok(entities);
    }
}
