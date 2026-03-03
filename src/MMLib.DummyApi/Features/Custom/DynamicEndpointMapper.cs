using MMLib.DummyApi.Features.Custom.Models;
using MMLib.DummyApi.Infrastructure;
using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Maps dynamic endpoints for collections after they are loaded.
/// </summary>
public static class DynamicEndpointMapper
{
    /// <summary>
    /// Maps CRUD endpoints for a specific collection definition.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="definition">The collection definition to map endpoints for.</param>
    public static void MapCollectionEndpoints(this IEndpointRouteBuilder app, CollectionDefinition definition)
    {
        var collectionName = definition.Name.ToLowerInvariant();
        var displayName = definition.DisplayName ?? definition.Name;
        var tag = displayName;

        var group = app.MapGroup($"/{collectionName}")
            .WithTags(tag);

        if (!string.IsNullOrEmpty(definition.Description))
        {
            group.WithDescription(definition.Description);
        }

        var getListEndpoint = group.MapGet("/", (CustomCollectionService service, RuleResolver ruleResolver, HttpContext httpContext) =>
        {
            if (definition.AuthRequired && !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var ruleResponse = ruleResolver.TryMatchRule(definition.Rules, "GET", httpContext);
            if (ruleResponse != null)
            {
                return ApplyRuleResponse(ruleResponse, httpContext);
            }

            var entities = service.GetAll(collectionName);
            return Results.Ok(entities);
        })
        .WithName($"Get{displayName}List")
        .WithSummary($"Get all {displayName}")
        .Produces(StatusCodes.Status200OK);

        ConfigureAuthProduces(getListEndpoint, definition.AuthRequired);

        var getByIdEndpoint = group.MapGet("/{id:guid}", (Guid id, CustomCollectionService service, RuleResolver ruleResolver, HttpContext httpContext) =>
        {
            if (definition.AuthRequired && !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var ruleResponse = ruleResolver.TryMatchRule(definition.Rules, "GET", httpContext);
            if (ruleResponse != null)
            {
                return ApplyRuleResponse(ruleResponse, httpContext);
            }

            var entity = service.GetById(collectionName, id);
            if (entity == null)
            {
                return Results.NotFound(new { error = $"{displayName} not found" });
            }
            return Results.Ok(entity.Value);
        })
        .WithName($"Get{displayName}ById")
        .WithSummary($"Get {displayName} by ID")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        ConfigureAuthProduces(getByIdEndpoint, definition.AuthRequired);

        var postEndpoint = group.MapPost("/", async (JsonElement data, CustomCollectionService service, BackgroundJobService backgroundJobService, RuleResolver ruleResolver, HttpContext httpContext) =>
        {
            if (definition.AuthRequired && !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var ruleResponse = ruleResolver.TryMatchRule(definition.Rules, "POST", httpContext, data);
            if (ruleResponse != null)
            {
                return ApplyRuleResponse(ruleResponse, httpContext);
            }

            var (entity, errors) = service.Create(collectionName, data);
            if (entity == null)
            {
                return Results.BadRequest(new { errors });
            }

            var id = Guid.Parse(entity.Value.GetProperty("id").GetString()!);

            var config = service.GetBackgroundConfig(collectionName);
            if (config != null)
            {
                var delayMs = GetBackgroundDelay(httpContext, config.DelayMs);
                backgroundJobService.ScheduleCustomJob(collectionName, id, delayMs);
            }

            return Results.Created($"/{collectionName}/{id}", entity.Value);
        })
        .WithName($"Create{displayName}")
        .WithSummary($"Create {displayName}")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        ConfigureAuthProduces(postEndpoint, definition.AuthRequired);

        var putEndpoint = group.MapPut("/{id:guid}", (Guid id, JsonElement data, CustomCollectionService service, RuleResolver ruleResolver, HttpContext httpContext) =>
        {
            if (definition.AuthRequired && !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var ruleResponse = ruleResolver.TryMatchRule(definition.Rules, "PUT", httpContext, data);
            if (ruleResponse != null)
            {
                return ApplyRuleResponse(ruleResponse, httpContext);
            }

            var (entity, errors) = service.Update(collectionName, id, data);
            if (entity == null)
            {
                if (errors.Any(e => e.Contains("not found")))
                {
                    return Results.NotFound(new { error = $"{displayName} not found" });
                }
                return Results.BadRequest(new { errors });
            }

            return Results.Ok(entity.Value);
        })
        .WithName($"Update{displayName}")
        .WithSummary($"Update {displayName}")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        ConfigureAuthProduces(putEndpoint, definition.AuthRequired);

        var deleteEndpoint = group.MapDelete("/{id:guid}", (Guid id, CustomCollectionService service, RuleResolver ruleResolver, HttpContext httpContext) =>
        {
            if (definition.AuthRequired && !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var ruleResponse = ruleResolver.TryMatchRule(definition.Rules, "DELETE", httpContext);
            if (ruleResponse != null)
            {
                return ApplyRuleResponse(ruleResponse, httpContext);
            }

            if (!service.Delete(collectionName, id))
            {
                return Results.NotFound(new { error = $"{displayName} not found" });
            }

            return Results.NoContent();
        })
        .WithName($"Delete{displayName}")
        .WithSummary($"Delete {displayName}")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        ConfigureAuthProduces(deleteEndpoint, definition.AuthRequired);
    }

    /// <summary>
    /// Returns the background job delay from the request header, or the default if not present.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="defaultDelay">The default delay in milliseconds.</param>
    internal static int GetBackgroundDelay(HttpContext httpContext, int defaultDelay)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Background-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delay))
        {
            return delay;
        }
        return defaultDelay;
    }

    private static void ConfigureAuthProduces(RouteHandlerBuilder endpoint, bool authRequired)
    {
        if (authRequired)
        {
            endpoint.Produces(StatusCodes.Status401Unauthorized);
        }
    }

    private static IResult ApplyRuleResponse(RuleResponse response, HttpContext httpContext)
    {
        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                httpContext.Response.Headers[header.Key] = header.Value;
            }
        }

        if (response.DelayMs.HasValue && response.DelayMs.Value > 0)
        {
            Thread.Sleep(response.DelayMs.Value);
        }

        if (response.Body.HasValue)
        {
            return Results.Json(response.Body.Value, statusCode: response.StatusCode);
        }

        return Results.StatusCode(response.StatusCode);
    }
}
