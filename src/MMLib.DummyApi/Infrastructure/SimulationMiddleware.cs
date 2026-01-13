using System.Collections.Concurrent;

namespace MMLib.DummyApi.Infrastructure;

public class SimulationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RetryState> _retryStates = new();

    public SimulationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Simulate-Error: true - always return 500
        if (context.Request.Headers.TryGetValue("X-Simulate-Error", out var simulateError) &&
            bool.TryParse(simulateError, out var shouldError) && shouldError)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Simulated error" });
            return;
        }

        // X-Simulate-Retry: N - first N-1 requests return 500
        if (context.Request.Headers.TryGetValue("X-Simulate-Retry", out var retryHeader) &&
            int.TryParse(retryHeader, out var retryCount) && retryCount > 0)
        {
            var requestId = context.Request.Headers["X-Request-Id"].ToString();
            if (string.IsNullOrWhiteSpace(requestId))
            {
                requestId = Guid.NewGuid().ToString();
                context.Request.Headers["X-Request-Id"] = requestId;
            }

            var key = $"{context.Request.Path}:{requestId}";
            var state = _retryStates.GetOrAdd(key, _ => new RetryState(retryCount));

            if (state.AttemptCount < retryCount - 1)
            {
                state.AttemptCount++;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Simulated retry error", attempt = state.AttemptCount });
                return;
            }

            // Last attempt - remove state and continue
            _retryStates.TryRemove(key, out _);
        }

        // X-Chaos-FailureRate: 0.3 - 30% chance of 500
        if (context.Request.Headers.TryGetValue("X-Chaos-FailureRate", out var failureRateHeader) &&
            double.TryParse(failureRateHeader, out var failureRate) && failureRate > 0)
        {
            var random = Random.Shared.NextDouble();
            if (random < failureRate)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Chaos failure", rate = failureRate });
                return;
            }
        }

        // X-Simulate-Delay: 500 - delay response
        if (context.Request.Headers.TryGetValue("X-Simulate-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delayMs) && delayMs > 0)
        {
            await Task.Delay(delayMs, context.RequestAborted);
        }

        // X-Chaos-LatencyRange: 100-500 - random delay in range
        if (context.Request.Headers.TryGetValue("X-Chaos-LatencyRange", out var latencyRangeHeader))
        {
            var range = latencyRangeHeader.ToString();
            var parts = range.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var minMs) &&
                int.TryParse(parts[1], out var maxMs) &&
                minMs >= 0 && maxMs >= minMs)
            {
                var randomDelay = Random.Shared.Next(minMs, maxMs + 1);
                await Task.Delay(randomDelay, context.RequestAborted);
            }
        }

        await _next(context);
    }
}

public class RetryState
{
    public int AttemptCount { get; set; }
    public int MaxRetries { get; }

    public RetryState(int maxRetries)
    {
        MaxRetries = maxRetries;
    }
}

public static class SimulationMiddlewareExtensions
{
    public static IApplicationBuilder UseSimulation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SimulationMiddleware>();
    }
}
