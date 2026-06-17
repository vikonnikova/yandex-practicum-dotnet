using Events.Application.Interfaces;
using Events.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.BackgroundServices;

internal class BookingBackgroundService(
	IBookingRepository bookingStore,
	IEventRepository eventStore,
	ILogger<BookingBackgroundService> logger)
	: BackgroundService
{
	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Фоновая обработка бронирований запущена.");

		while (!stoppingToken.IsCancellationRequested)
		{
			var pendingBookings = bookingStore.GetPending();
			var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
			await Task.WhenAll(tasks);

			await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
		}

		logger.LogInformation("Фоновая обработка бронирований остановлена.");
	}

	private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
	{
		logger.LogInformation("Начало обработки бронирования с идентификатором {BookingId}", booking.Id);

		try
		{
			// Имитация долгой обработки
			await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

			await _processingSemaphore.WaitAsync(stoppingToken);
			try
			{
				var @event = eventStore.Find(booking.EventId);

				if (@event is null)
				{
					booking.Reject(DateTime.UtcNow);
					logger.LogWarning(
						"Бронирование с идентификатором {BookingId} отклонено. Не найдено событие с идентификатором {EventId}.",
						booking.Id, booking.EventId);
				}
				else
				{
					booking.Confirm(DateTime.UtcNow);
				}
			}
			finally
			{
				_processingSemaphore.Release();
			}
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			logger.LogWarning("Обработка бронирования с идентификатором {BookingId} отменена.", booking.Id);
			return;
		}
		catch (Exception ex)
		{
			booking.Reject(DateTime.UtcNow);
			var @event = eventStore.Find(booking.EventId);
			@event!.ReleaseSeats();

			logger.LogError(ex, "Ошибка при обработке бронирования.");
		}

		logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно.", booking.Id);
	}
}