using System.Net;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(DummyApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthEndpoint_ContainsTimestamp()
    {
        var response = await _client.GetAsync("/health");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("timestamp", out var timestamp));
        Assert.True(DateTime.TryParse(timestamp.GetString(), out _));
    }
}
