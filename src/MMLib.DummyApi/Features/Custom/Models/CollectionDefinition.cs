using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMLib.DummyApi.Features.Custom.Models;

/// <summary>
/// Definition of a custom collection with all its configuration
/// </summary>
public record CollectionDefinition
{
    /// <summary>
    /// Unique name of the collection (used in URL path)
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Display name for OpenAPI documentation
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Description for OpenAPI documentation
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// JSON Schema for validation (optional)
    /// </summary>
    [JsonPropertyName("schema")]
    public JsonElement? Schema { get; init; }

    /// <summary>
    /// Whether this collection requires API key authentication
    /// </summary>
    public bool AuthRequired { get; init; } = false;

    /// <summary>
    /// Number of items to seed with AutoBogus on creation
    /// </summary>
    public int SeedCount { get; init; } = 0;

    /// <summary>
    /// Background job configuration for this collection
    /// </summary>
    public BackgroundJobConfig? BackgroundJob { get; init; }

    /// <summary>
    /// Response rules (mockoon-style templates with conditions)
    /// </summary>
    public List<ResponseRule>? Rules { get; init; }

    /// <summary>
    /// Timestamp when the collection was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
