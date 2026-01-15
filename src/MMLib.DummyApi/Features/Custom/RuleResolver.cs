using System.Text.Json;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Resolves rules for requests and returns matching responses
/// </summary>
public class RuleResolver
{
    /// <summary>
    /// Try to match a request against collection rules
    /// Returns the matching rule response if found, null otherwise (fall back to CRUD)
    /// </summary>
    public RuleResponse? TryMatchRule(
        List<ResponseRule>? rules,
        string httpMethod,
        HttpContext httpContext,
        JsonElement? requestBody = null)
    {
        if (rules == null || rules.Count == 0)
            return null;

        // Sort rules by priority
        var sortedRules = rules.OrderBy(r => r.Priority).ToList();

        foreach (var rule in sortedRules)
        {
            // Check method match
            if (rule.Method != "*" && !rule.Method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
                continue;

            // Check all conditions
            var allConditionsMatch = true;
            foreach (var condition in rule.When)
            {
                if (!EvaluateCondition(condition, httpContext, requestBody))
                {
                    allConditionsMatch = false;
                    break;
                }
            }

            if (allConditionsMatch)
            {
                return rule.Response;
            }
        }

        return null;
    }

    private bool EvaluateCondition(RuleCondition condition, HttpContext httpContext, JsonElement? requestBody)
    {
        var value = GetValueFromSource(condition.Source, condition.Field, httpContext, requestBody);
        
        if (value == null)
        {
            return condition.Operator == "notExists";
        }

        return condition.Operator switch
        {
            "exists" => true,
            "notExists" => false,
            "equals" => value.Equals(condition.Value, StringComparison.OrdinalIgnoreCase),
            "contains" => value.Contains(condition.Value, StringComparison.OrdinalIgnoreCase),
            "startsWith" => value.StartsWith(condition.Value, StringComparison.OrdinalIgnoreCase),
            "endsWith" => value.EndsWith(condition.Value, StringComparison.OrdinalIgnoreCase),
            "greaterThan" => CompareNumeric(value, condition.Value, (a, b) => a > b),
            "lessThan" => CompareNumeric(value, condition.Value, (a, b) => a < b),
            "range" => CheckRange(value, condition.Value),
            _ => false
        };
    }

    private string? GetValueFromSource(string source, string field, HttpContext httpContext, JsonElement? requestBody)
    {
        return source.ToLowerInvariant() switch
        {
            "query" => httpContext.Request.Query.TryGetValue(field, out var queryValue) ? queryValue.ToString() : null,
            "header" => httpContext.Request.Headers.TryGetValue(field, out var headerValue) ? headerValue.ToString() : null,
            "path" => GetPathValue(httpContext, field),
            "body" => GetJsonValue(requestBody, field),
            _ => null
        };
    }

    private string? GetPathValue(HttpContext httpContext, string field)
    {
        if (httpContext.Request.RouteValues.TryGetValue(field, out var value))
        {
            return value?.ToString();
        }
        return null;
    }

    private string? GetJsonValue(JsonElement? element, string path)
    {
        if (element == null)
            return null;

        var current = element.Value;
        foreach (var part in path.Split('.'))
        {
            if (current.ValueKind != JsonValueKind.Object)
                return null;

            if (!current.TryGetProperty(part, out current))
                return null;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => current.GetRawText()
        };
    }

    private bool CompareNumeric(string value, string compareValue, Func<decimal, decimal, bool> comparer)
    {
        if (decimal.TryParse(value, out var num1) && decimal.TryParse(compareValue, out var num2))
        {
            return comparer(num1, num2);
        }
        return false;
    }

    private bool CheckRange(string value, string rangeValue)
    {
        var parts = rangeValue.Split(',');
        if (parts.Length != 2)
            return false;

        if (decimal.TryParse(value, out var num) &&
            decimal.TryParse(parts[0].Trim(), out var min) &&
            decimal.TryParse(parts[1].Trim(), out var max))
        {
            return num >= min && num <= max;
        }
        return false;
    }
}
