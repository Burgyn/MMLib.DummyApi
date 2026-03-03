using System.Collections.Concurrent;

namespace MMLib.DummyApi.Infrastructure;

/// <summary>
/// Middleware that simulates various failure and latency scenarios based on request headers.
/// </summary>
public class SimulationMiddleware(RequestDelegate next)
{
    private readonly ConcurrentDictionary<string, RetryState> _retryStates = new();

    /// <summary>
    /// Processes the HTTP request and applies configured simulations before passing to the next middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Simulate-Error", out var simulateError) &&
            bool.TryParse(simulateError, out var shouldError) && shouldError)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Simulated error" });
            return;
        }

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

            _retryStates.TryRemove(key, out _);
        }

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

        if (context.Request.Headers.TryGetValue("X-Simulate-Delay", out var delayHeader) &&
            int.TryParse(delayHeader, out var delayMs) && delayMs > 0)
        {
            await Task.Delay(delayMs, context.RequestAborted);
        }

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

        await next(context);
    }
}

/// <summary>
/// Tracks the retry attempt state for a specific request.
/// </summary>
public class RetryState(int maxRetries)
{
    /// <summary>
    /// Number of attempts made so far.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Maximum number of retries configured for this request.
    /// </summary>
    public int MaxRetries { get; } = maxRetries;
}

/// <summary>
/// Extension methods for registering <see cref="SimulationMiddleware"/>.
/// </summary>
public static class SimulationMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="SimulationMiddleware"/> to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static IApplicationBuilder UseSimulation(this IApplicationBuilder app)
        => app.UseMiddleware<SimulationMiddleware>();
}
