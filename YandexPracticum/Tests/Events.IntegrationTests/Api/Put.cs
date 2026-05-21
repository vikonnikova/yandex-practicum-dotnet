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
		await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());

		//Act
		var response = await Client.PutAsJsonAsync("/events/1", TestData.CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		var updatedEvent = (await Client.GetFromJsonAsync<EventDto>("/events/1"))!;
		Assert.Equal(1, updatedEvent.Id);
		Assert.Equal(TestData.UpdatedTitle, updatedEvent.Title);
		Assert.Equal(TestData.UpdatedDescription, updatedEvent.Description);
		Assert.Equal(TestData.UpdatedStartAt, updatedEvent.StartAt);
		Assert.Equal(TestData.UpdatedEndAt, updatedEvent.EndAt);
	}
}