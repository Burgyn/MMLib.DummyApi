await tp.Test("Created collection definition should contain the correct name.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.name);
    Equal("tasks", (string)response.name);
});
