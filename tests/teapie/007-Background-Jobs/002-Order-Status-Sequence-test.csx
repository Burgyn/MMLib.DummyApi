await tp.Test("Created order should have a valid id and pending status.", async () =>
{
    dynamic response = await tp.Responses["CreateOrderForBgJobRequest"].GetBodyAsExpandoAsync();
    NotNull(response.id);
    True(Guid.TryParse((string)response.id, out _));
    Equal("pending", (string)response.status);
});

await tp.Test("Status should be completed", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.status);
    Equal("completed", (string)response.status);
});
