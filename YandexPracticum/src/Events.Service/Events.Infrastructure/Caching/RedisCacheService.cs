using System.Text.Json;
using Events.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Events.Infrastructure.Caching;

internal sealed class RedisCacheService(
    IConnectionMultiplexer multiplexer,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var value = await multiplexer.GetDatabase().StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка чтения из Redis по ключу {CacheKey}. Запрос будет выполнен без кеша.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await multiplexer.GetDatabase().StringSetAsync(key, payload, ttl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка записи в Redis по ключу {CacheKey}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await multiplexer.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка удаления ключа {CacheKey} из Redis.", key);
        }
    }
}
