namespace MMLib.DummyApi.Features.Performance;

/// <summary>
/// Thread-safe counter for performance testing endpoints.
/// </summary>
public class PerformanceCounter
{
    private long _counter = 0;

    /// <summary>
    /// Returns the current counter value.
    /// </summary>
    public long Get() => Interlocked.Read(ref _counter);

    /// <summary>
    /// Atomically increments the counter and returns the new value.
    /// </summary>
    public long Increment() => Interlocked.Increment(ref _counter);

    /// <summary>
    /// Resets the counter to zero.
    /// </summary>
    public void Reset() => Interlocked.Exchange(ref _counter, 0);
}
