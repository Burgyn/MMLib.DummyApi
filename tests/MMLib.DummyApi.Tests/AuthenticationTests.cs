using System.Net;

namespace MMLib.DummyApi.Tests;

[Collection("Api")]
public class AuthenticationTests
{
    private readonly DummyApiFixture _fixture;
    private readonly HttpClient _client;

    public AuthenticationTests(DummyApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task OrdersWithoutApiKey_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdersWithValidApiKey_ReturnsOk()
    {
        using var authClient = _fixture.CreateAuthenticatedClient();

        var response = await authClient.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrdersWithInvalidApiKey_ReturnsUnauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/orders");
        request.Headers.Add("X-Api-Key", "wrong-key");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProductsWithoutApiKey_ReturnsOk()
    {
        var response = await _client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
