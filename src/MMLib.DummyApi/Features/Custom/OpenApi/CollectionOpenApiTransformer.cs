using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom.OpenApi;

/// <summary>
/// OpenAPI transformer that adds collection schemas and request body examples.
/// </summary>
public class CollectionOpenApiTransformer(
    CustomDataStore dataStore,
    AutoBogusSeeder seeder,
    IConfiguration configuration,
    IOptions<DummyApiOptions> options,
    ILogger<CollectionOpenApiTransformer> logger)
    : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private readonly CustomDataStore _dataStore = dataStore;
    private readonly AutoBogusSeeder _seeder = seeder;
    private readonly IConfiguration _configuration = configuration;
    private readonly IOptions<DummyApiOptions> _options = options;
    private readonly ILogger<CollectionOpenApiTransformer> _logger = logger;

    /// <summary>
    /// Transforms the OpenAPI document by adding collection schemas to components/schemas.
    /// </summary>
    /// <param name="document">The OpenAPI document to transform.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var definitions = GetCollectionDefinitions();

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        foreach (var definition in definitions)
        {
            if (definition.Schema == null) continue;

            try
            {
                var schemaName = JsonSchemaConverter.ToPascalCase(definition.Name);
                var openApiSchema = JsonSchemaConverter.Convert(definition.Schema.Value);

                document.Components.Schemas[schemaName] = openApiSchema;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add schema for collection {CollectionName}", definition.Name);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Transforms an OpenAPI operation by adding request/response schemas and examples.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to transform.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var path = context.Description.RelativePath;
        if (string.IsNullOrEmpty(path)) return Task.CompletedTask;

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return Task.CompletedTask;
        var collectionName = parts[0];

        var definition = _dataStore.GetDefinition(collectionName);
        if (definition?.Schema == null) return Task.CompletedTask;

        var method = context.Description.HttpMethod;
        var isListEndpoint = parts.Length == 1;

        var itemSchema = JsonSchemaConverter.Convert(definition.Schema.Value);

        try
        {
            switch (method)
            {
                case "GET":
                    if (isListEndpoint)
                        SetArrayResponse(operation, itemSchema, definition, 200);
                    else
                    {
                        SetSingleItemResponse(operation, itemSchema, definition, 200);
                        SetErrorResponse(operation, 404, "Not Found", "error");
                    }
                    break;

                case "POST":
                    SetRequestBody(operation, itemSchema, definition);
                    SetSingleItemResponse(operation, itemSchema, definition, 201);
                    SetValidationErrorResponse(operation, 400);
                    break;

                case "PUT":
                    SetRequestBody(operation, itemSchema, definition);
                    SetSingleItemResponse(operation, itemSchema, definition, 200);
                    SetValidationErrorResponse(operation, 400);
                    SetErrorResponse(operation, 404, "Not Found", "error");
                    break;

                case "DELETE":
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

    private void SetArrayResponse(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition, int statusCode)
    {
        var arraySchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = itemSchema
        };

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

    private void SetSingleItemResponse(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition, int statusCode)
    {
        var examples = _seeder.Generate(definition.Schema, 1);
        JsonNode? exampleJson = null;
        if (examples.Count > 0)
        {
            exampleJson = JsonNode.Parse(examples[0].GetRawText());
        }

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

    private void SetRequestBody(OpenApiOperation operation, IOpenApiSchema itemSchema, CollectionDefinition definition)
    {
        var examples = _seeder.Generate(definition.Schema, 1);
        JsonNode? exampleJson = null;
        if (examples.Count > 0)
        {
            exampleJson = JsonNode.Parse(examples[0].GetRawText());
        }

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

    private void EnsureResponse(OpenApiOperation operation, string statusCode)
    {
        operation.Responses ??= new OpenApiResponses();

        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses[statusCode] = new OpenApiResponse();
        }
    }

    private List<CollectionDefinition> GetCollectionDefinitions()
    {
        var definitions = _dataStore.GetAllDefinitions().ToList();

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
                definitions = collectionsFile?.Collections ?? [];
            }
        }

        return definitions;
    }
}
