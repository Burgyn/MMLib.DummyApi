await tp.Test("After retries, response should be a product list.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();

    True(responseBody.TrimStart().StartsWith("["));

    var items = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(responseBody);
    NotNull(items);
});
