await tp.Test("Missing name should return validation errors.", async () =>
{
    dynamic response = await tp.Responses["CreateProductMissingName"].GetBodyAsExpandoAsync();
    NotNull(response.errors);
});

await tp.Test("Invalid price should return validation errors.", async () =>
{
    dynamic response = await tp.Responses["CreateProductInvalidPrice"].GetBodyAsExpandoAsync();
    NotNull(response.errors);
});

await tp.Test("Name too short should return validation errors.", async () =>
{
    dynamic response = await tp.Responses["CreateProductNameTooShort"].GetBodyAsExpandoAsync();
    NotNull(response.errors);
});
