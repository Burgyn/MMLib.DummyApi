await tp.Test("Rule response should contain the simulated error message.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
    Contains("category=error", (string)response.error);
});
