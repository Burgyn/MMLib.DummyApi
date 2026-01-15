using System.Text.Json;
using System.Text.Json.Nodes;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace MMLib.DummyApi.Features.Custom.OpenApi;

/// <summary>
/// OpenAPI transformer that adds collection schemas and request body examples
/// </summary>
public class CollectionOpenApiTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private readonly CustomDataStore _dataStore;
    private readonly AutoBogusSeeder _seeder;
    private readonly IConfiguration _configuration;
    private readonly IOptions<DummyApiOptions> _options;
    private readonly ILogger<CollectionOpenApiTransformer> _logger;

    public CollectionOpenApiTransformer(
        CustomDataStore dataStore,
        AutoBogusSeeder seeder,
        IConfiguration configuration,
        IOptions<DummyApiOptions> options,
        ILogger<CollectionOpenApiTransformer> logger)
    {
        _dataStore = dataStore;
        _seeder = seeder;
        _configuration = configuration;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Transform OpenAPI document - add collection schemas to components/schemas
    /// </summary>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var definitions = GetCollectionDefinitions();
        
        // Initialize components if needed
        if (document.Components == null)
        {
            document.Components = new OpenApiComponents();
        }
        if (document.Components.Schemas == null)
        {
            document.Components.Schemas = new Dictionary<string, IOpenApiSchema>();
        }
        
        foreach (var definition in definitions)
        {
            if (definition.Schema == null) continue;
            
            try
            {
                // Convert JSON Schema to OpenApiSchema and add to components
                var schemaName = JsonSchemaConverter.ToPascalCase(definition.Name); // "products" -> "Product"
                var openApiSchema = JsonSchemaConverter.Convert(definition.Schema.Value);
                
                // Add to components/schemas
                document.Components.Schemas[schemaName] = openApiSchema;
            }
            catch (Exception ex)
            {
                // Log error but continue with other schemas
                _logger.LogWarning(ex, "Failed to add schema for collection {CollectionName}", definition.Name);
            }
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Transform OpenAPI operation - add request body examples for POST/PUT endpoints
    /// </summary>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Get collection name from the path
        var path = context.Description.RelativePath;
        if (string.IsNullOrEmpty(path)) return Task.CompletedTask;
        
        // Extract collection name from path (e.g., "/products" -> "products")
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return Task.CompletedTask;
        var collectionName = parts[0];
        
        var method = context.Description.HttpMethod;
        
        // Only process POST and PUT operations for examples
        if (method != "POST" && method != "PUT") return Task.CompletedTask;
        
        // Get collection definition
        var definition = _dataStore.GetDefinition(collectionName);
        if (definition?.Schema == null) return Task.CompletedTask;
        
        // Generate example
        var examples = _seeder.Generate(definition.Schema, 1);
        if (examples.Count > 0)
        {
            // Set example on request body
            if (operation.RequestBody?.Content != null &&
                operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
            {
                var exampleJson = JsonNode.Parse(examples[0].GetRawText());
                if (exampleJson != null)
                {
                    mediaType.Example = exampleJson;
                }
            }
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get collection definitions from dataStore or file
    /// </summary>
    private List<CollectionDefinition> GetCollectionDefinitions()
    {
        // Try to get definitions from dataStore first
        var definitions = _dataStore.GetAllDefinitions().ToList();
        
        // If no definitions in dataStore, try loading from file directly
        if (definitions.Count == 0)
        {
            var filePath = _options.Value.CollectionsFile;
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(AppContext.BaseDirectory, "collections.json");
            }
            
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var collectionsFile = JsonSerializer.Deserialize<CollectionsFile>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                definitions = collectionsFile?.Collections ?? new List<CollectionDefinition>();
            }
        }
        
        return definitions;
    }
}
