using System.Net;
using System.Net.Http.Json;
using Events.Application.UseCases.Dto;

namespace Events.IntegrationTests.Api;

public class PostTests : BaseApiTest
{
	[Fact]
	public async Task Post_Success()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());

		//Assert
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal("/Events/1", response.Headers.Location!.AbsolutePath);

		var responseData = (await response.Content.ReadFromJsonAsync<EventDto>())!;
		Assert.Equal(1, responseData.Id);
		Assert.Equal(TestData.Title, responseData.Title);
		Assert.Equal(TestData.Description, responseData.Description);
		Assert.Equal(TestData.StartAt, responseData.StartAt);
		Assert.Equal(TestData.EndAt, responseData.EndAt);

		var createdEvent = (await Client.GetFromJsonAsync<EventDto>("/events/1"))!;
		Assert.Equal(1, createdEvent.Id);
		Assert.Equal(TestData.Title, createdEvent.Title);
		Assert.Equal(TestData.Description, createdEvent.Description);
		Assert.Equal(TestData.StartAt, createdEvent.StartAt);
		Assert.Equal(TestData.EndAt, createdEvent.EndAt);
	}
}