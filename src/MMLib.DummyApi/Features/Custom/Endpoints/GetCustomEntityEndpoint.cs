using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving a specific entity from a collection.
/// </summary>
public static class GetCustomEntityEndpoint
{
    /// <summary>
    /// Maps the GET /{collection}/{id} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCustomEntity(this IEndpointRouteBuilder app)
        => app.MapGet("/{collection}/{id:guid}", Handle)
            .WithName("GetCustomEntity")
            .WithSummary("Get a specific entity from a collection");

    private static IResult Handle(
        string collection,
        Guid id,
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

        JsonElement? entity = service.GetById(collection, id);
        if (entity == null)
        {
            return Results.NotFound(new { error = $"Entity not found in collection '{collection}'" });
        }

        return Results.Ok(entity.Value);
    }
}
