await tp.Test("VIP response body should contain VIP customer data.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();

    True(responseBody.TrimStart().StartsWith("["));
    Contains("vip-001", responseBody);
    Contains("VIP", responseBody);
});

tp.Test("VIP response should include the custom header.", () =>
{
    True(tp.Response.Headers.Contains("X-Custom-Header"));
    Equal("VIP-Response", tp.Response.Headers.GetValues("X-Custom-Header").First());
});
