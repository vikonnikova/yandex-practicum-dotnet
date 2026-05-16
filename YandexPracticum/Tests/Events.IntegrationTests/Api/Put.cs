using System.Net;
using System.Net.Http.Json;
using Events.Application.UseCases.Dto;

namespace Events.IntegrationTests.Api;

public class PutTests : BaseApiTest
{
	[Fact]
	public async Task Put_Success()
	{
		//Arrange
		var @event = new
		{
			Id = 1,
			Title = "Наименование",
			Description = "Описание",
			StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc)
		};
		await Client.PostAsJsonAsync("/events", @event);

		var eventToUpdate = new
		{
			Title = "Новое наименование",
			Description = "Новое описание",
			StartAt = new DateTime(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc)
		};

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{@event.Id}", eventToUpdate);

		//Assert
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		var updatedEvent = (await Client.GetFromJsonAsync<EventDto>($"/events/{@event.Id}"))!;
		Assert.Equal(@event.Id, updatedEvent.Id);
		Assert.Equal(eventToUpdate.Title, updatedEvent.Title);
		Assert.Equal(eventToUpdate.Description, updatedEvent.Description);
		Assert.Equal(eventToUpdate.StartAt, updatedEvent.StartAt);
		Assert.Equal(eventToUpdate.EndAt, updatedEvent.EndAt);
	}
}