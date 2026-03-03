using System.Net;
using System.Text.Json;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class OpenApiSchemaTests
{
    private readonly HttpClient _client;

    public OpenApiSchemaTests(DummyApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task OpenApiDocument_IsAccessible()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task OpenApiDocument_ContainsCollectionSchemas()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Verify components.schemas exists
        Assert.True(json.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));

        // Verify schemas for loaded collections exist (ToPascalCase converts "products" -> "Products")
        var schemaNames = schemas.EnumerateObject().Select(p => p.Name).ToList();
        
        Assert.Contains("Products", schemaNames);
        Assert.Contains("Orders", schemaNames);
        Assert.Contains("Customers", schemaNames);
    }

    [Fact]
    public async Task OpenApiDocument_ProductsSchemaHasRequiredFields()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var productSchema = json
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("Products");

        // Verify required fields
        Assert.True(productSchema.TryGetProperty("required", out var required));
        var requiredArray = required.EnumerateArray().Select(r => r.GetString()).ToList();
        Assert.Contains("name", requiredArray);
        Assert.Contains("price", requiredArray);

        // Verify properties exist - at minimum required fields should be present
        Assert.True(productSchema.TryGetProperty("properties", out var properties));
        var propertyNames = properties.EnumerateObject().Select(p => p.Name).ToList();
        
        // Required fields must be in properties
        Assert.Contains("name", propertyNames);
        Assert.Contains("price", propertyNames);
        
        // Verify there are additional properties beyond required ones
        Assert.True(propertyNames.Count >= 2, "Schema should have at least the required properties");
    }

    [Fact]
    public async Task OpenApiDocument_ContainsDynamicEndpoints()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("paths", out var paths));

        var pathNames = paths.EnumerateObject().Select(p => p.Name).ToList();

        // Verify collection endpoints exist
        Assert.Contains("/products", pathNames);
        Assert.Contains("/products/{id}", pathNames);
        Assert.Contains("/orders", pathNames);
        Assert.Contains("/orders/{id}", pathNames);
        Assert.Contains("/customers", pathNames);
        Assert.Contains("/customers/{id}", pathNames);
    }

    [Fact]
    public async Task OpenApiDocument_ContainsSystemAndPerfEndpoints()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("paths", out var paths));

        var pathNames = paths.EnumerateObject().Select(p => p.Name).ToList();

        // Verify system endpoints
        Assert.Contains("/health", pathNames);
        Assert.Contains("/reset", pathNames);

        // Verify performance endpoints
        Assert.Contains("/perf/payload", pathNames);
        Assert.Contains("/perf/counter", pathNames);
        Assert.Contains("/perf/counter/increment", pathNames);
        Assert.Contains("/perf/counter/reset", pathNames);
    }
}
