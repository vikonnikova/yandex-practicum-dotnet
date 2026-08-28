using Auth.IntegrationTests.Api.Base;

namespace Events.IntegrationTests.Api.Base;

[Collection("Api Collection")]
public abstract class BaseApiTest(ApiFixture fixture) : IAsyncLifetime
{
    protected readonly ApiFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.Client;

    public async Task InitializeAsync()
    {
        await Fixture.ClearTablesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}