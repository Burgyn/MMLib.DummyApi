using System.Text.Json;
using Json.Schema;

namespace MMLib.DummyApi.Features.Custom;

public class JsonSchemaValidator
{
    /// <summary>
    /// Validate data against a provided schema
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate(string collection, JsonElement data, JsonElement schema)
    {
        try
        {
            var jsonSchema = JsonSchema.FromText(schema.GetRawText());
            var result = jsonSchema.Evaluate(data);

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

    /// <summary>
    /// Validate that a schema is a valid JSON Schema
    /// </summary>
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
        // Collect errors from this result
        if (result.Errors != null && result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                var path = result.InstanceLocation.ToString();
                var errorMessage = error.Value ?? error.Key ?? "Validation error";
                errors.Add($"{path}: {errorMessage}");
            }
        }
        
        // Also check if the result itself indicates failure
        if (!result.IsValid && result.Errors == null)
        {
            var path = result.InstanceLocation.ToString();
            errors.Add($"{path}: Validation failed");
        }

        // Recursively collect errors from child results
        if (result.Details != null)
        {
            foreach (var detail in result.Details)
            {
                CollectErrorsRecursive(detail, errors);
            }
        }
    }
}
