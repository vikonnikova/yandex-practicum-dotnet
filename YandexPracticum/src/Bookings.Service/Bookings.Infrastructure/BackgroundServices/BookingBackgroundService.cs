using Bookings.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.BackgroundServices;

internal class BookingBackgroundService(
    //TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<BookingBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(10);

    // TODO подумать над архитектурой, чтобы покрыть тестами механизм отклонения заявок.
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

            //await TryConfirm(booking, eventStore, cancellationToken);
            await bookingStore.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно.", bookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке бронирования.");
            //await Reject(booking, eventStore, cancellationToken);
            await bookingStore.SaveChangesAsync(cancellationToken);
        }
    }

    /*private async Task TryConfirm(Booking booking, CancellationToken cancellationToken)
    {
        if (@event is null)
        {
            booking.Reject(timeProvider.GetUtcNow().UtcDateTime);
        }
        else
        {
            booking.Confirm(timeProvider.GetUtcNow().UtcDateTime);
        }
    }

    private async Task Reject(Booking booking, CancellationToken cancellationToken)
    {
        booking.Reject(timeProvider.GetUtcNow().UtcDateTime);
        var @event = await eventStore.Find(booking.EventId, cancellationToken);
        @event?.ReleaseSeats();
    }*/
}