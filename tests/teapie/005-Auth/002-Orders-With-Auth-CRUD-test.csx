await tp.Test("Created order should match request data.", async () =>
{
    dynamic request = await tp.Requests["CreateOrderRequest"].GetBodyAsExpandoAsync();
    dynamic response = await tp.Responses["CreateOrderRequest"].GetBodyAsExpandoAsync();

    Equal((string)request.customerId, (string)response.customerId);
    Equal((double)request.totalAmount, (double)response.totalAmount);
    NotNull(response.id);
});

await tp.Test("Retrieved order should match created order.", async () =>
{
    dynamic created = await tp.Responses["CreateOrderRequest"].GetBodyAsExpandoAsync();
    dynamic retrieved = await tp.Responses["GetOrderRequest"].GetBodyAsExpandoAsync();

    Equal((string)created.id, (string)retrieved.id);
    Equal((string)created.customerId, (string)retrieved.customerId);
});
