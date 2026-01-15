using System.Text.Json;

namespace MMLib.DummyApi.Features.Custom.Models;

/// <summary>
/// A mockoon-style response rule with conditions and template response
/// </summary>
public record ResponseRule
{
    /// <summary>
    /// Rule priority (lower number = higher priority)
    /// </summary>
    public int Priority { get; init; } = 100;
    
    /// <summary>
    /// HTTP method this rule applies to (GET, POST, PUT, DELETE, or * for all)
    /// </summary>
    public string Method { get; init; } = "*";
    
    /// <summary>
    /// Conditions that must be met for this rule to match
    /// </summary>
    public List<RuleCondition> When { get; init; } = new();
    
    /// <summary>
    /// The response to return when conditions match
    /// </summary>
    public RuleResponse Response { get; init; } = new();
}

/// <summary>
/// A condition for matching requests
/// </summary>
public record RuleCondition
{
    /// <summary>
    /// Source of the value to check: query, body, header, path
    /// </summary>
    public string Source { get; init; } = "query";
    
    /// <summary>
    /// Path to the field (e.g., "id", "status", "user.name")
    /// </summary>
    public string Field { get; init; } = string.Empty;
    
    /// <summary>
    /// Operator: equals, contains, startsWith, endsWith, greaterThan, lessThan, range, exists, notExists
    /// </summary>
    public string Operator { get; init; } = "equals";
    
    /// <summary>
    /// Value to compare against (can be string, number, or for range: "min,max")
    /// </summary>
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Response template for a rule
/// </summary>
public record RuleResponse
{
    /// <summary>
    /// HTTP status code to return
    /// </summary>
    public int StatusCode { get; init; } = 200;
    
    /// <summary>
    /// Response body as JSON (can use templates like {{id}}, {{faker.name}})
    /// </summary>
    public JsonElement? Body { get; init; }
    
    /// <summary>
    /// Custom headers to add to the response
    /// </summary>
    public Dictionary<string, string>? Headers { get; init; }
    
    /// <summary>
    /// Delay in milliseconds before returning the response
    /// </summary>
    public int? DelayMs { get; init; }
}
