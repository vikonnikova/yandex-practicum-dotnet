using System.Net;
using System.Net.Http.Json;
using Events.Application.UseCases.Dto;

namespace Events.IntegrationTests.Api;

public class PutTests : BaseApiTest
{
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Put_ValidData_204Returned()
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
	
	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Put_InvalidData_400Returned()
	{
		//Arrange
		await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());

		//Act
		var response = await Client.PutAsJsonAsync("/events/1", TestData.CreateInvalidTestEventToUpdate());

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
		await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());

		//Act
		var response = await Client.PutAsJsonAsync("/events/2", TestData.CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}