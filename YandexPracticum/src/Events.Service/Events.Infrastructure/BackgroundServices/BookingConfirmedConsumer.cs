using System.Text.Json;
using Confluent.Kafka;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Settings;

namespace Events.Infrastructure.BackgroundServices;

internal sealed class BookingConfirmedConsumer(
    IServiceProvider serviceProvider,
    KafkaSettings settings,
    ILogger<BookingConfirmedConsumer> logger)
    : BackgroundService
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
    {
        BootstrapServers = settings.BootstrapServers,
        GroupId = settings.ConsumerGroup,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    }).Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(KafkaConstants.BookingConfirmedTopic);
        logger.LogInformation("Подписка на топик Kafka '{Topic}' запущена.", KafkaConstants.BookingConfirmedTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult?.Message?.Value is null)
                {
                    continue;
                }

                var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value);
                if (bookingConfirmed is null)
                {
                    logger.LogError("Не удалось десериализовать сообщение из топика '{Topic}'.",
                        KafkaConstants.BookingConfirmedTopic);
                    continue;
                }

                using var scope = serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var @event = await repository.Find(bookingConfirmed.EventId, stoppingToken);
                if (@event is null)
                {
                    throw new EntityNotFoundException("Событие", bookingConfirmed.EventId);
                }

                if (!@event.TryReserveSeats(bookingConfirmed.SeatsCount))
                {
                    throw new NoAvailableSeatsException();
                }

                await repository.SaveChangesAsync(stoppingToken);
                await cache.RemoveAsync(CacheKeys.Event(bookingConfirmed.EventId), stoppingToken);

                logger.LogInformation(
                    "Подтверждённая бронь {BookingId} обработана для события {EventId}, мест: {SeatsCount}.",
                    bookingConfirmed.BookingId,
                    bookingConfirmed.EventId,
                    bookingConfirmed.SeatsCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке сообщения из Kafka. Сообщение будет пропущено.");
            }
        }

        logger.LogInformation("Подписка на топик Kafka '{Topic}' остановлена.", KafkaConstants.BookingConfirmedTopic);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}