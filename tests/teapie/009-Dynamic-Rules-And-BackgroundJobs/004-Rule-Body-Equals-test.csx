await tp.Test("Body equals rule should return blocked-type error.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
    Equal("blocked-type", (string)response.error);
});
