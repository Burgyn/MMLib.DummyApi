await tp.Test("Response should be a non-empty JSON array.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();

    True(responseBody.TrimStart().StartsWith("["));
    True(responseBody.TrimEnd().EndsWith("]"));

    var items = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(responseBody);
    NotNull(items);
    True(items.Length > 0);
});
