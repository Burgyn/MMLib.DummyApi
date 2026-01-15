using Microsoft.AspNetCore.Http.HttpResults;

namespace MMLib.DummyApi.Features.Custom.Endpoints;

public static class GetCollectionsEndpoint
{
    public static RouteHandlerBuilder MapGetCollections(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/", Handle)
            .WithName("GetCollections")
            .WithSummary("Get list of all collection names");
    }

    private static Ok<IEnumerable<string>> Handle(CustomCollectionService service)
    {
        var collections = service.GetCollections();
        return TypedResults.Ok(collections);
    }
}
