using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;

namespace Events.IntegrationTests.EventsController;

public class UpdateTests(ApiWebApplicationFactory factory) : BaseApiTest(factory)
{
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Put_ValidData_204Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{eventId}", TestData.CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		var updatedEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!;
		Assert.Equal(eventId, updatedEvent.Id);
		Assert.Equal(TestData.UpdatedTitle, updatedEvent.Title);
		Assert.Equal(TestData.UpdatedDescription, updatedEvent.Description);
		Assert.Equal(TestData.UpdatedStartAt, updatedEvent.StartAt);
		Assert.Equal(TestData.UpdatedEndAt, updatedEvent.EndAt);
	}
	
	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Put_InvalidData_400Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{eventId}", TestData.CreateInvalidTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
	
	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Put_NonExistentEvent_404Returned()
	{
		//Arrange
		await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{Guid.NewGuid()}", TestData.CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}