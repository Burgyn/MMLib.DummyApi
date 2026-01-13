using System.Collections.Concurrent;

namespace MMLib.DummyApi.Features.Performance;

public class PerformanceCounter
{
    private long _counter = 0;

    public long Get() => Interlocked.Read(ref _counter);

    public long Increment() => Interlocked.Increment(ref _counter);

    public void Reset() => Interlocked.Exchange(ref _counter, 0);
}
