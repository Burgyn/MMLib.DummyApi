using System.Text.Json;
using Json.Schema;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Validates JSON data against a JSON Schema.
/// </summary>
public class JsonSchemaValidator
{
    /// <summary>
    /// Validates data against a provided schema.
    /// </summary>
    /// <param name="collection">The collection name (used for error context).</param>
    /// <param name="data">The JSON data to validate.</param>
    /// <param name="schema">The JSON schema to validate against.</param>
    public (bool IsValid, List<string> Errors) Validate(string collection, JsonElement data, JsonElement schema)
    {
        try
        {
            var jsonSchema = JsonSchema.FromText(schema.GetRawText());
            var result = jsonSchema.Evaluate(data);

            if (result.IsValid)
            {
                return (true, []);
            }

            var errors = CollectErrors(result);
            return (false, errors);
        }
        catch (Exception ex)
        {
            return (false, [$"Schema validation error: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Validates that the given element is a well-formed JSON Schema.
    /// </summary>
    /// <param name="schema">The schema element to validate.</param>
    public (bool IsValid, string? Error) ValidateSchema(JsonElement schema)
    {
        try
        {
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
        List<string> errors = [];
        CollectErrorsRecursive(result, errors);
        return errors;
    }

    private static void CollectErrorsRecursive(EvaluationResults result, List<string> errors)
    {
        if (result.Errors != null && result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                var path = result.InstanceLocation.ToString();
                var errorMessage = error.Value ?? error.Key ?? "Validation error";
                errors.Add($"{path}: {errorMessage}");
            }
        }

        if (!result.IsValid && result.Errors == null)
        {
            var path = result.InstanceLocation.ToString();
            errors.Add($"{path}: Validation failed");
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
