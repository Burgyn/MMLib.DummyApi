using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class CollectionDefinitionTests : IAsyncLifetime
{
    private readonly DummyApiFixture _fixture;
    private readonly HttpClient _client;

    public CollectionDefinitionTests(DummyApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync() => await _fixture.ResetDataAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDefinition_ReturnsCreated()
    {
        var definition = new
        {
            name = "testcol",
            displayName = "Test Collection",
            description = "A test collection",
            seedCount = 0,
            schema = new
            {
                type = "object",
                required = new[] { "title" },
                properties = new
                {
                    title = new { type = "string" }
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/custom/_definitions", definition);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("testcol", json.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetAllDefinitions_IncludesLoadedCollections()
    {
        var response = await _client.GetAsync("/custom/_definitions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var definitions = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(definitions);

        var names = definitions.Select(d => d.GetProperty("name").GetString()).ToList();
        Assert.Contains("products", names);
        Assert.Contains("orders", names);
        Assert.Contains("customers", names);
    }

    [Fact]
    public async Task GetDefinitionByName_ReturnsCorrectDefinition()
    {
        var response = await _client.GetAsync("/custom/_definitions/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("products", json.GetProperty("name").GetString());
        Assert.Equal("Products", json.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task UpdateDefinition_ModifiesDescription()
    {
        var updated = new
        {
            name = "products",
            displayName = "Products",
            description = "Updated description",
            authRequired = false,
            seedCount = 50,
            schema = new
            {
                type = "object",
                required = new[] { "name", "price" },
                properties = new
                {
                    name = new { type = "string", minLength = 2 },
                    price = new { type = "number", minimum = 0.01 }
                }
            }
        };

        var response = await _client.PutAsJsonAsync("/custom/_definitions/products", updated);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated description", json.GetProperty("description").GetString());
    }

    [Fact]
    public async Task DeleteDefinition_RemovesCollection()
    {
        // First, create a temporary collection
        var definition = new
        {
            name = "tempdelete",
            displayName = "Temp Delete",
            seedCount = 0
        };
        await _client.PostAsJsonAsync("/custom/_definitions", definition);

        // Delete it
        var deleteResponse = await _client.DeleteAsync("/custom/_definitions/tempdelete");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's gone
        var getResponse = await _client.GetAsync("/custom/_definitions/tempdelete");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateDefinitionWithSeedCount_CanQuerySeededData()
    {
        const int seedCount = 5;
        var definition = new
        {
            name = "seededtest",
            displayName = "Seeded Test Collection",
            description = "Collection with seeded data",
            seedCount = seedCount,
            schema = new
            {
                type = "object",
                required = new[] { "name" },
                properties = new
                {
                    name = new { type = "string", minLength = 2 },
                    value = new { type = "number" }
                }
            }
        };

        // Create collection with seedCount
        var createResponse = await _client.PostAsJsonAsync("/custom/_definitions", definition);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Query the collection and verify it contains seeded data
        var getResponse = await _client.GetAsync("/custom/seededtest");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var items = await getResponse.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(items);
        Assert.Equal(seedCount, items.Length);

        // Verify each item has required properties
        foreach (var item in items)
        {
            Assert.True(item.TryGetProperty("id", out _), "Item should have 'id' property");
            Assert.True(item.TryGetProperty("name", out _), "Item should have 'name' property");
        }
    }
}
