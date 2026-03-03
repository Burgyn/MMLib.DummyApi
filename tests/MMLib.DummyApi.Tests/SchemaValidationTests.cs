using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class SchemaValidationTests : IAsyncLifetime
{
    private readonly DummyApiFixture _fixture;
    private readonly HttpClient _client;

    public SchemaValidationTests(DummyApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync() => await _fixture.ResetDataAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateWithoutRequiredFields_ReturnsBadRequest()
    {
        // Products require "name" and "price"
        var incomplete = new { description = "Missing required fields" };

        var response = await _client.PostAsJsonAsync("/products", incomplete);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task CreateWithInvalidType_ReturnsBadRequest()
    {
        // Price should be a number, not a string
        var invalid = new { name = "Bad Product", price = "not-a-number" };

        var response = await _client.PostAsJsonAsync("/products", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInNonexistentCollection_ReturnsNotFound()
    {
        var data = new { name = "test" };

        var response = await _client.PostAsJsonAsync("/custom/nonexistent", data);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
