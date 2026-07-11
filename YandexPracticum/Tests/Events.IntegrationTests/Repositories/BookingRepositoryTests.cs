using Events.Infrastructure.DataAccess;
using Events.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class BookingRepositoryTests(DbFixture dbFixture)
{
	private async Task<AppDbContext> CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(dbFixture.ConnectionString)
			.Options;

		return new AppDbContext(options);
	}

	[Fact]
	public void AddEvent_ShouldSaveToDb()
	{
		using var context = CreateContext();
		// Ваш тест репозитория...
		Assert.True(true);
	}
}