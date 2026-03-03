using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class PerformanceEndpointTests
{
    private readonly HttpClient _client;

    public PerformanceEndpointTests(DummyApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetPayloadBySize_ReturnsPayload()
    {
        var response = await _client.GetAsync("/perf/payload?size=1kb");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1024, json.GetProperty("size").GetInt32());
    }

    [Fact]
    public async Task GetPayloadByItems_ReturnsCorrectCount()
    {
        var response = await _client.GetAsync("/perf/payload?items=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, json.GetProperty("count").GetInt32());
        Assert.Equal(5, json.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task CounterLifecycle_IncrementGetReset()
    {
        // Reset counter first
        await _client.PostAsync("/perf/counter/reset", null);

        // Increment
        await _client.PostAsync("/perf/counter/increment", null);
        await _client.PostAsync("/perf/counter/increment", null);

        // Get counter
        var response = await _client.GetAsync("/perf/counter");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, json.GetProperty("value").GetInt64());

        // Reset
        await _client.PostAsync("/perf/counter/reset", null);

        var afterReset = await _client.GetAsync("/perf/counter");
        var resetJson = await afterReset.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, resetJson.GetProperty("value").GetInt64());
    }
}
