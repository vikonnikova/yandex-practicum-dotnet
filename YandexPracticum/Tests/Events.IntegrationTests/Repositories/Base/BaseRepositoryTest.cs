using Events.Domain;
using Events.IntegrationTests.Api.Base;

namespace Events.IntegrationTests.Repositories.Base;

public abstract class BaseRepositoryTest(DbFixture dbFixture) : IAsyncLifetime
{
	protected readonly DbFixture DbFixture = dbFixture;
	protected readonly DateTime Date = new(2022, 05, 04, 12, 00, 00, DateTimeKind.Utc);
	protected readonly Guid EventId = Guid.NewGuid();
	protected readonly Guid BookingId = Guid.NewGuid();

	protected Event CreateEvent()
	{
		return Event.Create(EventId, TestData.Title, TestData.Description,
			EventPeriod.Create(TestData.StartAt, TestData.EndAt), TestData.TotalSeats);
	}

	protected async Task SeedData()
	{
		await using var context = DbFixture.CreateContext();
		
		context.Events.AddRange(
			Event.Create(Guid.NewGuid(), "День рождения", "Дед Мороз и снегурочка",
				EventPeriod.Create(Date, Date.AddDays(7)), 17),
			Event.Create(Guid.NewGuid(), "Пасха", "Красим яйца, печем куличи",
				EventPeriod.Create(Date.AddHours(-12), Date.AddHours(-10)), 20),
			Event.Create(Guid.NewGuid(), "Рождество", "описание рождества, подарки, игрушки",
				EventPeriod.Create(Date.AddMonths(-5), Date.AddMonths(-5).AddDays(2)), 100),
			Event.Create(Guid.NewGuid(), "23 февраля", "День защитника отечества",
				EventPeriod.Create(Date.AddDays(-7), Date.AddDays(-6)), 5),
			Event.Create(Guid.NewGuid(), "День победы", "Парад и салют",
				EventPeriod.Create(Date, Date.AddHours(14)), 7)
		);

		await context.SaveChangesAsync();
	}

	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	public async Task DisposeAsync()
	{
		await DbFixture.ClearTablesAsync();
	}
}