await tp.Test("No-match fallback should return an array (normal CRUD).", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    True(responseBody.TrimStart().StartsWith("["));
});
