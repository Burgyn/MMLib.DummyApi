await tp.Test("Range rule should return inRange=true.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.inRange);
    True((bool)response.inRange);
});
