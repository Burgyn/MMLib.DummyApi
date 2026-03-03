using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for updating an entity in a collection.
/// </summary>
public static class PutCustomEntityEndpoint
{
    /// <summary>
    /// Maps the PUT /{collection}/{id} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapPutCustomEntity(this IEndpointRouteBuilder app)
        => app.MapPut("/{collection}/{id:guid}", Handle)
            .WithName("UpdateCustomEntity")
            .WithSummary("Update an entity in a collection");

    private static IResult Handle(
        string collection,
        Guid id,
        JsonElement data,
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
        Models.RuleResponse? ruleResponse = ruleResolver.TryMatchRule(rules, "PUT", httpContext, data);
        if (ruleResponse != null)
        {
            return DynamicEndpointMapper.ApplyRuleResponse(ruleResponse, httpContext);
        }

        (JsonElement? entity, List<string> errors) = service.Update(collection, id, data);

        if (entity == null)
        {
            if (errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new { error = $"Entity not found in collection '{collection}'" });
            }
            return Results.BadRequest(new { errors });
        }

        return Results.Ok(entity.Value);
    }
}
