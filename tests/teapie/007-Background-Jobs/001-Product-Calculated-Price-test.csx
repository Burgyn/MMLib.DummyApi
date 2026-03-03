await tp.Test("Created product should have a valid id.", async () =>
{
    dynamic response = await tp.Responses["CreateProductForBgJobRequest"].GetBodyAsExpandoAsync();
    NotNull(response.id);
    True(Guid.TryParse((string)response.id, out _));
});

await tp.Test("calculatedPrice should be set", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();

    True(responseBody.Contains("calculatedPrice"));
    NotNull(response.calculatedPrice);

    double price = (double)response.calculatedPrice;
    True(price >= 10 && price <= 100);
});
