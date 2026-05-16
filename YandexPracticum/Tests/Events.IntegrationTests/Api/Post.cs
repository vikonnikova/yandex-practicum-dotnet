using System.Net;
using System.Net.Http.Json;
using Events.Application.UseCases.Dto;

namespace Events.IntegrationTests.Api;

public class PostTests : BaseApiTest
{
	[Fact]
	public async Task Post_Success()
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

		//Act
		var response = await Client.PostAsJsonAsync("/events", @event);

		//Assert
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal($"/Events/{@event.Id}", response.Headers.Location!.AbsolutePath);

		var responseData = (await response.Content.ReadFromJsonAsync<EventDto>())!;
		Assert.Equal(@event.Id, responseData.Id);
		Assert.Equal(@event.Title, responseData.Title);
		Assert.Equal(@event.Description, responseData.Description);
		Assert.Equal(@event.StartAt, responseData.StartAt);
		Assert.Equal(@event.EndAt, responseData.EndAt);

		var createdEvent = (await Client.GetFromJsonAsync<EventDto>($"/events/{@event.Id}"))!;
		Assert.Equal(@event.Id, createdEvent.Id);
		Assert.Equal(@event.Title, createdEvent.Title);
		Assert.Equal(@event.Description, createdEvent.Description);
		Assert.Equal(@event.StartAt, createdEvent.StartAt);
		Assert.Equal(@event.EndAt, createdEvent.EndAt);
	}
}