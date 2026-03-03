await tp.Test("Header exists rule should return test mode body.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.mode);
    Equal("test", (string)response.mode);
});

tp.Test("Header exists rule should set X-Rule-Applied header.", () =>
{
    True(tp.Response.Headers.Contains("X-Rule-Applied"));
    Equal("true", tp.Response.Headers.GetValues("X-Rule-Applied").First());
});
