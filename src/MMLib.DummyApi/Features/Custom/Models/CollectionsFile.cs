namespace MMLib.DummyApi.Features.Custom.Models;

/// <summary>
/// Root structure for the collections JSON file loaded at startup
/// </summary>
public record CollectionsFile
{
    /// <summary>
    /// List of collection definitions
    /// </summary>
    public List<CollectionDefinition> Collections { get; init; } = [];
}
