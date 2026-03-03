namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for deleting an entity from a collection.
/// </summary>
public static class DeleteCustomEntityEndpoint
{
    /// <summary>
    /// Maps the DELETE /{collection}/{id} endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapDeleteCustomEntity(this IEndpointRouteBuilder app)
        => app.MapDelete("/{collection}/{id:guid}", Handle)
            .WithName("DeleteCustomEntity")
            .WithSummary("Delete an entity from a collection");

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
        Models.RuleResponse? ruleResponse = ruleResolver.TryMatchRule(rules, "DELETE", httpContext);
        if (ruleResponse != null)
        {
            return DynamicEndpointMapper.ApplyRuleResponse(ruleResponse, httpContext);
        }

        if (!service.Delete(collection, id))
        {
            return Results.NotFound(new { error = $"Entity not found in collection '{collection}'" });
        }

        return Results.NoContent();
    }
}
