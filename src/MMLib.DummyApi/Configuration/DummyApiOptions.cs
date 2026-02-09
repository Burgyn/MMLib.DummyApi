namespace MMLib.DummyApi.Configuration;

public class DummyApiOptions
{
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

public class PerformanceOptions
{
    public int MaxPayloadSizeMb { get; set; } = 10;
}
