await tp.Test("Retrieved product should match created product.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    string createdProduct = tp.GetVariable<string>("CreatedProduct");

    JsonContains(responseBody, createdProduct, "calculatedPrice");
});
