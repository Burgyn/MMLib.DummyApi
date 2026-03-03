await tp.Test("Updated product should reflect changes.", async () =>
{
    dynamic request = await tp.Request.GetBodyAsExpandoAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();

    Equal((string)request.name, (string)response.name);
    Equal((double)request.price, (double)response.price);
    Equal((string)request.description, (string)response.description);
});
