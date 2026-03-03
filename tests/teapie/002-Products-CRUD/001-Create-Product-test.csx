await tp.Test("Response should contain request data.", async () =>
{
    string requestBody = await tp.Request.Content.ReadAsStringAsync();
    string responseBody = await tp.Response.Content.ReadAsStringAsync();

    JsonContains(responseBody, requestBody, "id", "calculatedPrice");
});

await tp.Test("Created product should have a valid id.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();

    NotNull(response.id);
    True(Guid.TryParse((string)response.id, out _));

    tp.SetVariable("ProductId", (string)response.id);
    tp.SetVariable("CreatedProduct", responseBody);
});
