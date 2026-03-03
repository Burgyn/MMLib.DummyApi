namespace MMLib.DummyApi.Features.Custom.Models;

public record BackgroundJobConfig
{
    /// <summary>
    /// Path to the field to update (e.g., "status", "calculatedTotal")
    /// </summary>
    public string FieldPath { get; init; } = string.Empty;

    /// <summary>
    /// Operation to perform:
    /// - sequence:val1,val2,val3 - cycles through values
    /// - sum:path.to.array.field - sum of values in array
    /// - count:path.to.array - count of items
    /// - timestamp - current UTC timestamp
    /// - random:min,max - random number
    /// </summary>
    public string Operation { get; init; } = string.Empty;

    /// <summary>
    /// Delay in milliseconds before executing the job
    /// </summary>
    public int DelayMs { get; init; } = 5000;
}
