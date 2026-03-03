await tp.Test("Created entity should have a valid id.", async () =>
{
    dynamic response = await tp.Responses["CreateEntityForBgJobRequest"].GetBodyAsExpandoAsync();
    NotNull(response.id);
    True(Guid.TryParse((string)response.id, out _));
});

await tp.Test("Status should be done", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();

    True(responseBody.Contains("status"));
    NotNull(response.status);
    Equal("done", (string)response.status);
});
