using Events.Application.Interfaces;
using Events.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.BackgroundServices;

internal class BookingBackgroundService(IBookingTaskQueue taskQueue, IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
	: BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Фоновая обработка бронирований запущена.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (taskQueue.TryDequeue(out var task))
				{
					logger.LogInformation("Начало обработки бронирования с идентификатором {BookingId}", task.BookingId);

					// Имитация долгой обработки
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
					
					using var scope = scopeFactory.CreateScope();
					var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
					service.Confirm(task.BookingId);

					logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно", task.BookingId);
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка при обработке бронирования.");
			}

			await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
		}

		logger.LogInformation("Фоновая обработка бронирований остановлена.");
	}
}