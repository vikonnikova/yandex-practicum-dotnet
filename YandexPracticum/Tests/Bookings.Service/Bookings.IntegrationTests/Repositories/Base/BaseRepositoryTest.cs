namespace Bookings.IntegrationTests.Repositories.Base;

public abstract class BaseRepositoryTest(DbFixture dbFixture) : IAsyncLifetime
{
    protected readonly DbFixture DbFixture = dbFixture;
    protected readonly DateTime Date = new(2022, 05, 04, 12, 00, 00, DateTimeKind.Utc);

    public async Task InitializeAsync()
    {
        await DbFixture.ClearTablesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}