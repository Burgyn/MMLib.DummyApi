namespace MMLib.DummyApi.Configuration;

/// <summary>
/// Configuration options for the DummyApi.
/// </summary>
public class DummyApiOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "DummyApi";

    /// <summary>
    /// Path to the JSON file with collection definitions (can be mounted in Docker)
    /// </summary>
    public string? CollectionsFile { get; set; }

    /// <summary>
    /// Default API key for authenticated collections
    /// </summary>
    public string DefaultApiKey { get; set; } = "test-api-key-123";

    /// <summary>
    /// Performance testing options
    /// </summary>
    public PerformanceOptions Performance { get; set; } = new();
}

/// <summary>
/// Performance-related configuration options.
/// </summary>
public class PerformanceOptions
{
    /// <summary>
    /// Maximum allowed payload size in megabytes.
    /// </summary>
    public int MaxPayloadSizeMb { get; set; } = 10;
}
