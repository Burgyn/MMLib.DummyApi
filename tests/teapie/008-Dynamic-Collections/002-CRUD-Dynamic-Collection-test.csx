await tp.Test("Created task should contain request data.", async () =>
{
    dynamic request = await tp.Requests["CreateTaskRequest"].GetBodyAsExpandoAsync();
    dynamic response = await tp.Responses["CreateTaskRequest"].GetBodyAsExpandoAsync();

    Equal((string)request.title, (string)response.title);
    Equal((string)request.priority, (string)response.priority);
    NotNull(response.id);
});

await tp.Test("Retrieved task should match created task.", async () =>
{
    dynamic created = await tp.Responses["CreateTaskRequest"].GetBodyAsExpandoAsync();
    dynamic retrieved = await tp.Responses["GetTaskRequest"].GetBodyAsExpandoAsync();

    Equal((string)created.id, (string)retrieved.id);
    Equal((string)created.title, (string)retrieved.title);
});

await tp.Test("Updated task should reflect changes.", async () =>
{
    dynamic request = await tp.Requests["UpdateTaskRequest"].GetBodyAsExpandoAsync();
    dynamic response = await tp.Responses["UpdateTaskRequest"].GetBodyAsExpandoAsync();

    Equal((string)request.title, (string)response.title);
    Equal((string)request.priority, (string)response.priority);
    Equal((bool)request.completed, (bool)response.completed);
});

await tp.Test("Tasks list should be a JSON array.", async () =>
{
    string responseBody = await tp.Responses["ListTasksRequest"].Content.ReadAsStringAsync();

    True(responseBody.TrimStart().StartsWith("["));

    var items = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(responseBody);
    NotNull(items);
});
