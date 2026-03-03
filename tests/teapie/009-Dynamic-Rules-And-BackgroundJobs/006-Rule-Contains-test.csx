await tp.Test("Contains rule should return filtered=true.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.filtered);
    True((bool)response.filtered);
});
