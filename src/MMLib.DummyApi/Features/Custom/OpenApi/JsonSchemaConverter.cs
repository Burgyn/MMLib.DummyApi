using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace MMLib.DummyApi.Features.Custom.OpenApi;

/// <summary>
/// Converts JSON Schema to OpenAPI Schema format
/// </summary>
public static class JsonSchemaConverter
{
    /// <summary>
    /// Convert JSON Schema (JsonElement) to OpenApiSchema
    /// </summary>
    public static IOpenApiSchema Convert(JsonElement schemaElement)
    {
        var schemaNode = JsonNode.Parse(schemaElement.GetRawText());
        if (schemaNode == null) return new OpenApiSchema();
        
        return ConvertNode(schemaNode);
    }

    /// <summary>
    /// Convert JSON Schema node to OpenApiSchema recursively
    /// </summary>
    private static IOpenApiSchema ConvertNode(JsonNode? schemaNode)
    {
        if (schemaNode == null) return new OpenApiSchema();
        
        var schema = new OpenApiSchema();
        
        // Handle type - convert string to JsonSchemaType enum
        if (schemaNode["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeStr))
        {
            schema.Type = typeStr switch
            {
                "string" => JsonSchemaType.String,
                "number" => JsonSchemaType.Number,
                "integer" => JsonSchemaType.Integer,
                "boolean" => JsonSchemaType.Boolean,
                "array" => JsonSchemaType.Array,
                "object" => JsonSchemaType.Object,
                _ => null
            };
        }
        
        // Handle properties
        if (schemaNode["properties"] is JsonObject properties)
        {
            schema.Properties = new Dictionary<string, IOpenApiSchema>();
            foreach (var prop in properties)
            {
                schema.Properties[prop.Key] = ConvertNode(prop.Value);
            }
        }
        
        // Handle required fields
        if (schemaNode["required"] is JsonArray requiredArray)
        {
            schema.Required = new HashSet<string>();
            foreach (var item in requiredArray)
            {
                if (item is JsonValue reqValue && reqValue.TryGetValue<string>(out var req))
                {
                    schema.Required.Add(req);
                }
            }
        }
        
        // Handle items (for arrays)
        if (schemaNode["items"] is JsonNode itemsNode)
        {
            schema.Items = ConvertNode(itemsNode);
        }
        
        // Handle validation constraints
        if (schemaNode["minimum"] is JsonValue minValue && minValue.TryGetValue<decimal>(out var minimum))
        {
            schema.Minimum = minimum.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        }
        
        if (schemaNode["maximum"] is JsonValue maxValue && maxValue.TryGetValue<decimal>(out var maximum))
        {
            schema.Maximum = maximum.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        }
        
        if (schemaNode["minLength"] is JsonValue minLenValue && minLenValue.TryGetValue<int>(out var minLength))
        {
            schema.MinLength = minLength;
        }
        
        if (schemaNode["maxLength"] is JsonValue maxLenValue && maxLenValue.TryGetValue<int>(out var maxLength))
        {
            schema.MaxLength = maxLength;
        }
        
        if (schemaNode["format"] is JsonValue formatValue && formatValue.TryGetValue<string>(out var format))
        {
            schema.Format = format;
        }
        
        if (schemaNode["pattern"] is JsonValue patternValue && patternValue.TryGetValue<string>(out var pattern))
        {
            schema.Pattern = pattern;
        }
        
        // Handle enum - enum values are preserved in the original JSON Schema
        // Note: Microsoft.OpenApi 2.0.0 may handle enum differently, so we keep it in the original format
        
        if (schemaNode["description"] is JsonValue descValue && descValue.TryGetValue<string>(out var description))
        {
            schema.Description = description;
        }
        
        return schema;
    }

    /// <summary>
    /// Convert collection name to PascalCase schema name (e.g., "products" -> "Product")
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }
}
