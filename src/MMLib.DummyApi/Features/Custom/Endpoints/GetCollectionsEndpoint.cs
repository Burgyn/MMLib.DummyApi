using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

/// <summary>
/// Endpoint for retrieving the list of all collection names.
/// </summary>
public static class GetCollectionsEndpoint
{
    /// <summary>
    /// Maps the GET / endpoint that lists all collection names.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static RouteHandlerBuilder MapGetCollections(this IEndpointRouteBuilder app)
        => app.MapGet("/", Handle)
            .WithName("GetCollections")
            .WithSummary("Get list of all collection names");

    private static Ok<IEnumerable<string>> Handle(CustomCollectionService service)
    {
        var collections = service.GetCollections();
        return TypedResults.Ok(collections);
    }
}
