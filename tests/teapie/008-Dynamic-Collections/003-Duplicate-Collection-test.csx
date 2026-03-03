await tp.Test("Duplicate collection error should mention the collection name.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.error);
    Contains("tasks", (string)response.error);
    Contains("already exists", (string)response.error);
});
