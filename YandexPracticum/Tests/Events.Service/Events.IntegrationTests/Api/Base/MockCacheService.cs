using Events.Application.Interfaces;

namespace Events.IntegrationTests.Api.Base;

internal sealed class CacheServiceMock : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        return Task.FromResult(default(T?));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}