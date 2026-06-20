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
	private readonly TimeSpan _delayTimeSpan = TimeSpan.FromSeconds(2);
	private readonly TimeSpan _processBookingDelayTimeSpan = TimeSpan.FromSeconds(10);
	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
	private readonly SemaphoreSlim _rejectionSemaphore = new(1, 1);

	// TODO подумать над архитектурой, чтобы покрыть тестами механизм отклонения заявок.
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Фоновая обработка бронирований запущена.");

		while (!stoppingToken.IsCancellationRequested)
		{
			var pendingBookings = bookingStore.GetPending();
			var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
			await Task.WhenAll(tasks);

			await Task.Delay(_delayTimeSpan, stoppingToken);
		}

		logger.LogInformation("Фоновая обработка бронирований остановлена.");
	}

	private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
	{
		logger.LogInformation("Начало обработки бронирования с идентификатором {BookingId}", booking.Id);

		try
		{
			await TryConfirm(booking, stoppingToken);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			logger.LogWarning("Обработка бронирования с идентификатором {BookingId} отменена.", booking.Id);
			return;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при обработке бронирования.");
			await Reject(booking, stoppingToken);
		}

		logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно.", booking.Id);
	}

	private async Task TryConfirm(Booking booking, CancellationToken stoppingToken)
	{
		// Имитация долгой обработки
		await Task.Delay(_processBookingDelayTimeSpan, stoppingToken);

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

	private async Task Reject(Booking booking, CancellationToken stoppingToken)
	{
		await _rejectionSemaphore.WaitAsync(stoppingToken);
		try
		{
			booking.Reject(DateTime.UtcNow);
			var @event = eventStore.Find(booking.EventId);
			@event?.ReleaseSeats();
		}
		finally
		{
			_rejectionSemaphore.Release();
		}
	}
}