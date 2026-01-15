using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCollectionsEndpoint
{
    public static RouteHandlerBuilder MapGetCollections(this IEndpointRouteBuilder app)
    {
        return app.MapGet("", Handle)
            .WithName("GetCollections")
            .WithSummary("Get all custom collection names");
    }

    private static Ok<CollectionsResponse> Handle(CustomCollectionService service)
    {
        var collections = service.GetCollections();
        return TypedResults.Ok(new CollectionsResponse { Collections = collections.ToList() });
    }
}

public record CollectionsResponse
{
    public List<string> Collections { get; init; } = new();
}
