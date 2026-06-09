using Events.Application.UseCases;
using Events.Application.UseCases.Dto;

namespace Events.Api.Extensions;

internal static class DataInitializer
{
	public static void SeedData(this IApplicationBuilder app)
	{
		/*using (var scope = app.ApplicationServices.CreateScope())
		{
			var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

			eventService.Add(new EventDto(Guid.NewGuid(), "Новый год", "Дед Мороз и снегурочка",
				DateTime.UtcNow, DateTime.UtcNow.AddDays(7)));
			eventService.Add(new EventDto(Guid.NewGuid(), "Рождество", "описание рождества, подарки, игрушки",
				DateTime.UtcNow.AddMonths(-5), DateTime.UtcNow.AddMonths(-5).AddDays(2)));
			eventService.Add(new EventDto(Guid.NewGuid(), "Пасха", "Красим яйца, печем куличи",
				DateTime.UtcNow.AddHours(-12), DateTime.UtcNow.AddHours(-10)));
			eventService.Add(new EventDto(Guid.NewGuid(), "23 февраля", "День защитника отечества",
				DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-6)));
			eventService.Add(new EventDto(Guid.NewGuid(), "День победы", "Парад и салют",
				DateTime.UtcNow, DateTime.UtcNow.AddHours(14)));
		}*/
	}
}