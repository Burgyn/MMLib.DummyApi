namespace MMLib.DummyApi.Configuration;

public class DummyApiOptions
{
    public const string SectionName = "DummyApi";

    public int InitialProductCount { get; set; } = 50;
    public int InitialOrderCount { get; set; } = 20;
    public string DefaultApiKey { get; set; } = "test-api-key-123";
    public int BackgroundJobDelayMs { get; set; } = 2000;
    public PerformanceOptions Performance { get; set; } = new();
}

public class PerformanceOptions
{
    public int MaxPayloadSizeMb { get; set; } = 10;
    public int MaxDelayMs { get; set; } = 30000;
}
