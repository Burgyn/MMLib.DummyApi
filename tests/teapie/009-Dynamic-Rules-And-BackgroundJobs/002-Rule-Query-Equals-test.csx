await tp.Test("Query equals rule should return the error body.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
    Equal("rule-triggered", (string)response.error);
});
