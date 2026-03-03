await tp.Test("Wildcard method rule should return price-too-high error.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
    Equal("price-too-high", (string)response.error);
});
