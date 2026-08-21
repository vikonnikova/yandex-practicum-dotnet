using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.Api.Contracts.Bookings;
using Events.Api.Contracts.Events;

namespace Events.IntegrationTests.Api.Base;

[Collection("Api Collection")]
public abstract class BaseApiTest(ApiFixture fixture) : IAsyncLifetime
{
	protected readonly HttpClient Client = fixture.Client;

	protected static object CreateTestEvent()
	{
		return new { TestData.Title, TestData.Description, TestData.StartAt, TestData.EndAt, TestData.TotalSeats };
	}

	protected static object CreateInvalidTestEvent()
	{
		return new { TestData.Title, TestData.Description, TestData.StartAt, TestData.EndAt };
	}

	protected static object CreateTestEventToUpdate()
	{
		return new
		{
			Title = TestData.UpdatedTitle,
			Description = TestData.UpdatedDescription,
			StartAt = TestData.UpdatedStartAt,
			EndAt = TestData.UpdatedEndAt,
			TotalSeats = TestData.UpdatedTotalSeats
		};
	}

	protected static object CreateInvalidTestEventToUpdate()
	{
		return new
		{
			Description = TestData.UpdatedDescription,
			StartAt = TestData.UpdatedStartAt,
			EndAt = TestData.UpdatedEndAt,
			TotalSeats = TestData.UpdatedTotalSeats
		};
	}

	private static object[] CreateTestEvents()
	{
		return
		[
			new
			{
				Title = "Оперетта",
				Description =
					"Музыкально-театральный жанр, сочетающий вокальное и драматическое искусство, хореографию и разговорные диалоги",
				StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc),
				TotalSeats = 30
			},

			new
			{
				Title = "Балет",
				Description =
					"Театральный спектакль, в котором сюжет, характеры и эмоции героев передаются без слов — с помощью танца, пластики и музыки",
				StartAt = new DateTime(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc),
				TotalSeats = 40
			},
			new
			{
				Title = "Кукольный театр",
				Description = "Форма театра или представления, в которой используются куклы",
				StartAt = new DateTime(2026, 03, 03, 15, 32, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 03, 03, 17, 52, 00, DateTimeKind.Utc),
				TotalSeats = 50
			}
		];
	}

	protected async Task<Guid> CreateEvent()
	{
		var response = await Client.PostAsJsonAsync("/events", CreateTestEvent());
		var responseData = (await response.Content.ReadFromJsonAsync<EventResponse>())!;

		return responseData.Id;
	}

	protected async Task CreateEvents()
	{
		foreach (var @event in CreateTestEvents())
		{
			await Client.PostAsJsonAsync("/events", @event);
		}
	}

	protected async Task<Guid> CreateBooking(Guid eventId)
	{
		var response = await Client.PostAsync($"/events/{eventId}/book", null);
		var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;

		return booking.BookingId;
	}

	protected async Task CreateBookings(Guid eventId, int bookCount)
	{
		var tasks = Enumerable.Range(0, bookCount)
			.Select(_ => Task.Run(async () => { await Client.PostAsync($"/events/{eventId}/book", null); })).ToArray();

		await Task.WhenAll(tasks);
	}

	public async Task InitializeAsync()
	{
		await fixture.ClearTablesAsync();
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}
}