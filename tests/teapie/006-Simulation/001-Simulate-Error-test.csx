await tp.Test("Simulated error response should contain an error message.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
});
