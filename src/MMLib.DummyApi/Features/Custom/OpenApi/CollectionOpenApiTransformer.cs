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
    /// Transform OpenAPI operation - add request/response schemas and examples for all endpoints
    /// </summary>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Get collection name from the path
        var path = context.Description.RelativePath;
        if (string.IsNullOrEmpty(path)) return Task.CompletedTask;
        
        // Extract collection name from path (e.g., "/products" -> "products" or "/products/{id}" -> "products")
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return Task.CompletedTask;
        var collectionName = parts[0];
        
        // Get collection definition
        var definition = _dataStore.GetDefinition(collectionName);
        if (definition?.Schema == null) return Task.CompletedTask;
        
        var method = context.Description.HttpMethod;
        var isListEndpoint = parts.Length == 1; // Path like "/products" vs "/products/{id}"
        
        // Convert collection schema to OpenAPI schema
        var itemSchema = JsonSchemaConverter.Convert(definition.Schema.Value);
        
        try
        {
            switch (method)
            {
                case "GET":
                    if (isListEndpoint)
                    {
                        // GET list - array response
                        SetArrayResponse(operation, itemSchema, definition, 200);
                    }
                    else
                    {
                        // GET by ID - single item response
                        SetSingleItemResponse(operation, itemSchema, definition, 200);
                        SetErrorResponse(operation, 404, "Not Found", "error");
                    }
                    break;
                    
                case "POST":
                    // Request body schema and example
                    SetRequestBody(operation, itemSchema, definition);
                    // Response 201 Created - single item with id
                    SetSingleItemResponse(operation, itemSchema, definition, 201);
                    // Response 400 Bad Request
                    SetValidationErrorResponse(operation, 400);
                    break;
                    
                case "PUT":
                    // Request body schema and example
                    SetRequestBody(operation, itemSchema, definition);
                    // Response 200 OK - single item
                    SetSingleItemResponse(operation, itemSchema, definition, 200);
                    // Response 400 Bad Request
                    SetValidationErrorResponse(operation, 400);
                    // Response 404 Not Found
                    SetErrorResponse(operation, 404, "Not Found", "error");
                    break;
                    
                case "DELETE":
                    // Response 204 No Content (no body)
                    // Response 404 Not Found
                    SetErrorResponse(operation, 404, "Not Found", "error");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to transform OpenAPI operation for {Method} {Path}", method, path);
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Set array response schema and example (for GET list endpoints)
    /// </summary>
    private void SetArrayResponse(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition, int statusCode)
    {
        // Create array schema
        var arraySchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = itemSchema
        };
        
        // Generate multiple examples for array
        var examples = _seeder.Generate(definition.Schema, 3);
        var exampleArray = new JsonArray();
        foreach (var example in examples)
        {
            var exampleJson = JsonNode.Parse(example.GetRawText());
            if (exampleJson != null)
            {
                exampleArray.Add(exampleJson);
            }
        }
        
        // Set response - create new OpenApiResponse since Content is read-only
        var response = new OpenApiResponse
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = arraySchema,
                    Example = exampleArray.Count > 0 ? exampleArray : null
                }
            }
        };
        EnsureResponse(operation, statusCode.ToString());
        operation.Responses![statusCode.ToString()] = response;
    }
    
    /// <summary>
    /// Set single item response schema and example (for GET by ID, POST, PUT)
    /// </summary>
    private void SetSingleItemResponse(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition, int statusCode)
    {
        // Generate example
        var examples = _seeder.Generate(definition.Schema, 1);
        JsonNode? exampleJson = null;
        if (examples.Count > 0)
        {
            exampleJson = JsonNode.Parse(examples[0].GetRawText());
        }
        
        // Set response - create new OpenApiResponse since Content is read-only
        var response = new OpenApiResponse
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = itemSchema,
                    Example = exampleJson
                }
            }
        };
        EnsureResponse(operation, statusCode.ToString());
        operation.Responses![statusCode.ToString()] = response;
    }
    
    /// <summary>
    /// Set request body schema and example (for POST, PUT)
    /// </summary>
    private void SetRequestBody(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition)
    {
        // Generate example
        var examples = _seeder.Generate(definition.Schema, 1);
        JsonNode? exampleJson = null;
        if (examples.Count > 0)
        {
            exampleJson = JsonNode.Parse(examples[0].GetRawText());
        }
        
        // Set request body
        operation.RequestBody ??= new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>()
        };
        
        operation.RequestBody.Content!["application/json"] = new OpenApiMediaType
        {
            Schema = itemSchema,
            Example = exampleJson
        };
    }
    
    /// <summary>
    /// Set error response schema (for 404, etc.)
    /// </summary>
    private void SetErrorResponse(OpenApiOperation operation, int statusCode, string description, string errorPropertyName)
    {
        var errorSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [errorPropertyName] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "Error message"
                }
            },
            Required = new HashSet<string> { errorPropertyName }
        };
        
        var exampleJson = JsonObject.Parse($"{{\"{errorPropertyName}\": \"Resource not found\"}}");
        
        // Set response - create new OpenApiResponse since Content is read-only
        var response = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = errorSchema,
                    Example = exampleJson
                }
            }
        };
        EnsureResponse(operation, statusCode.ToString());
        operation.Responses![statusCode.ToString()] = response;
    }
    
    /// <summary>
    /// Set validation error response schema (for 400 Bad Request)
    /// </summary>
    private void SetValidationErrorResponse(OpenApiOperation operation, int statusCode)
    {
        var errorSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["errors"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    },
                    Description = "Validation error messages"
                }
            },
            Required = new HashSet<string> { "errors" }
        };
        
        var exampleJson = JsonObject.Parse("{\"errors\": [\"Field 'name' is required\", \"Field 'price' must be greater than 0\"]}");
        
        // Set response - create new OpenApiResponse since Content is read-only
        var response = new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = errorSchema,
                    Example = exampleJson
                }
            }
        };
        EnsureResponse(operation, statusCode.ToString());
        operation.Responses![statusCode.ToString()] = response;
    }
    
    /// <summary>
    /// Ensure response entry exists in operation
    /// </summary>
    private void EnsureResponse(OpenApiOperation operation, string statusCode)
    {
        if (operation.Responses == null)
        {
            operation.Responses = new OpenApiResponses();
        }
        
        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses[statusCode] = new OpenApiResponse();
        }
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
