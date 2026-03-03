using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class EntityCrudTests : IAsyncLifetime
{
    private readonly DummyApiFixture _fixture;
    private readonly HttpClient _client;

    public EntityCrudTests(DummyApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync() => await _fixture.ResetDataAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ReturnsSeededProducts()
    {
        var response = await _client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(products);
        Assert.Equal(50, products.Length);
    }

    [Fact]
    public async Task CreateEntity_ReturnsCreatedWithId()
    {
        var product = new
        {
            name = "Test Product",
            price = 19.99
        };

        var response = await _client.PostAsJsonAsync("/products", product);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("id", out var idProp));
        Assert.True(Guid.TryParse(idProp.GetString(), out _));
        Assert.Equal("Test Product", json.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetById_ReturnsCorrectEntity()
    {
        // Create an entity first
        var product = new { name = "Lookup Product", price = 5.50 };
        var createResponse = await _client.PostAsJsonAsync("/products", product);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        // Get by ID
        var response = await _client.GetAsync($"/products/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Lookup Product", json.GetProperty("name").GetString());
    }

    [Fact]
    public async Task UpdateEntity_ReturnsUpdatedData()
    {
        // Create an entity
        var product = new { name = "Original", price = 10.00 };
        var createResponse = await _client.PostAsJsonAsync("/products", product);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        // Update it
        var updated = new { name = "Updated", price = 25.00 };
        var response = await _client.PutAsJsonAsync($"/products/{id}", updated);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated", json.GetProperty("name").GetString());
        Assert.Equal(25.00, json.GetProperty("price").GetDouble());
    }

    [Fact]
    public async Task DeleteEntity_ReturnsNoContent()
    {
        // Create an entity
        var product = new { name = "To Delete", price = 1.00 };
        var createResponse = await _client.PostAsJsonAsync("/products", product);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        // Delete it
        var response = await _client.DeleteAsync($"/products/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedEntity_ReturnsNotFound()
    {
        // Create and delete an entity
        var product = new { name = "Ghost", price = 1.00 };
        var createResponse = await _client.PostAsJsonAsync("/products", product);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        await _client.DeleteAsync($"/products/{id}");

        // Try to get it
        var response = await _client.GetAsync($"/products/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
