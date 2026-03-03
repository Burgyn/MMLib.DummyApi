await tp.Test("StartsWith rule should return prefix=true.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.prefix);
    True((bool)response.prefix);
});
