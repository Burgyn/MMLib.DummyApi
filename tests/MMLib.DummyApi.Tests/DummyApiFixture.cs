using Microsoft.AspNetCore.Mvc.Testing;

namespace MMLib.DummyApi.Tests;

public class DummyApiFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

        Client = Factory.CreateClient();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    /// <summary>
    /// Reset all collections to their initial seeded state.
    /// </summary>
    public async Task ResetDataAsync()
    {
        await Client.PostAsync("/reset", null);
    }

    /// <summary>
    /// Create a client with the default API key for authenticated endpoints.
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-api-key-123");
        return client;
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<DummyApiFixture>
{
}
