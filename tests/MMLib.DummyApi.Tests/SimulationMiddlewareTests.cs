using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class SimulationMiddlewareTests
{
    private readonly HttpClient _client;

    public SimulationMiddlewareTests(DummyApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task SimulateError_Returns500()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Simulate-Error", "true");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Simulated error", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task SimulateRetry_FailsFirstThenSucceeds()
    {
        var requestId = Guid.NewGuid().ToString();

        // First request should fail (attempt 1 of 3)
        using var req1 = new HttpRequestMessage(HttpMethod.Get, "/health");
        req1.Headers.Add("X-Simulate-Retry", "3");
        req1.Headers.Add("X-Request-Id", requestId);
        var response1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.InternalServerError, response1.StatusCode);

        // Second request should also fail (attempt 2 of 3)
        using var req2 = new HttpRequestMessage(HttpMethod.Get, "/health");
        req2.Headers.Add("X-Simulate-Retry", "3");
        req2.Headers.Add("X-Request-Id", requestId);
        var response2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);

        // Third request should succeed (attempt 3 of 3)
        using var req3 = new HttpRequestMessage(HttpMethod.Get, "/health");
        req3.Headers.Add("X-Simulate-Retry", "3");
        req3.Headers.Add("X-Request-Id", requestId);
        var response3 = await _client.SendAsync(req3);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }

    [Fact]
    public async Task SimulateDelay_AddsDelay()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Simulate-Delay", "200");

        var sw = Stopwatch.StartNew();
        var response = await _client.SendAsync(request);
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(sw.ElapsedMilliseconds >= 150, $"Expected at least 150ms delay, got {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ChaosFailureRate100Percent_AlwaysFails()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Chaos-FailureRate", "1.0");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
