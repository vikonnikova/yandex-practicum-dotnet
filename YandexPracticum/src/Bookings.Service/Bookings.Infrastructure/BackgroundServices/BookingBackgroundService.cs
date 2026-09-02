using Bookings.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Bookings.Infrastructure.BackgroundServices;

internal class BookingBackgroundService(
    TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory,
    IKafkaPublisher kafkaPublisher,
    ILogger<BookingBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Фоновая обработка бронирований запущена.");

        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyCollection<Guid> pendingBookingsIds;
            using (var scope = scopeFactory.CreateScope())
            {
                var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                pendingBookingsIds = await bookingStore.GetPending(cancellationToken);
            }

            var tasks = pendingBookingsIds.Select(bookingId => ProcessBookingAsync(bookingId, cancellationToken));
            await Task.WhenAll(tasks);

            await Task.Delay(PollingInterval, cancellationToken);
        }

        logger.LogInformation("Фоновая обработка бронирований остановлена.");
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Начало обработки бронирования с идентификатором {BookingId}", bookingId);

        using var scope = scopeFactory.CreateScope();
        var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var booking = await bookingStore.Find(bookingId, cancellationToken);
        if (booking == null)
        {
            logger.LogWarning("Бронирование с идентификатором {BookingId} не найдено. Обработка отклонена.", bookingId);
            return;
        }

        try
        {
            await Task.Delay(ProcessingDelay, cancellationToken);
            var confirmedAt = timeProvider.GetUtcNow().UtcDateTime;
            booking.Confirm(confirmedAt);
            await bookingStore.SaveChangesAsync(cancellationToken);

            await kafkaPublisher.PublishBookingConfirmedAsync(
                new BookingConfirmedEvent(
                    booking.Id,
                    booking.EventId,
                    booking.UserId,
                    SeatsCount: 1,
                    ConfirmedAt: confirmedAt),
                cancellationToken);

            logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно.", bookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке бронирования.");
        }
    }
}