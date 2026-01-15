using System.Text.Json;
using Json.Schema;

namespace MMLib.DummyApi.Features.Custom;

public class JsonSchemaValidator
{
    private readonly CustomDataStore _dataStore;

    public JsonSchemaValidator(CustomDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public (bool IsValid, List<string> Errors) Validate(string collection, JsonElement data)
    {
        var schemaElement = _dataStore.GetSchema(collection);
        
        if (schemaElement == null)
        {
            // No schema defined - always valid
            return (true, new List<string>());
        }

        try
        {
            var schema = JsonSchema.FromText(schemaElement.Value.GetRawText());
            var result = schema.Evaluate(data);

            if (result.IsValid)
            {
                return (true, new List<string>());
            }

            var errors = CollectErrors(result);
            return (false, errors);
        }
        catch (Exception ex)
        {
            return (false, new List<string> { $"Schema validation error: {ex.Message}" });
        }
    }

    public (bool IsValid, string? Error) ValidateSchema(JsonElement schema)
    {
        try
        {
            // Try to parse the schema to validate it's a valid JSON Schema
            JsonSchema.FromText(schema.GetRawText());
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Invalid JSON Schema: {ex.Message}");
        }
    }

    private static List<string> CollectErrors(EvaluationResults result)
    {
        var errors = new List<string>();
        CollectErrorsRecursive(result, errors);
        return errors;
    }

    private static void CollectErrorsRecursive(EvaluationResults result, List<string> errors)
    {
        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
            {
                var path = result.InstanceLocation.ToString();
                errors.Add($"{path}: {error.Value}");
            }
        }

        if (result.Details != null)
        {
            foreach (var detail in result.Details)
            {
                CollectErrorsRecursive(detail, errors);
            }
        }
    }
}
