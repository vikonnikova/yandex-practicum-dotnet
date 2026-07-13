using Events.Application;
using Events.Domain;
using Events.Infrastructure;
using Events.IntegrationTests.Api.Base;
using Events.IntegrationTests.Repositories.Base;
using FluentAssertions;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class EventRepositoryTests(DbFixture dbFixture) : BaseRepositoryTest(dbFixture)
{
	/// <summary>
	/// Проверяет поиск события по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenValidData_ShouldReturnEvent()
	{
		// Arrange
		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var result = await new EventRepository(context).Find(EventId, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Id.Should().Be(EventId);
			result.Title.Should().Be(TestData.Title);
			result.Description.Should().Be(TestData.Description);
			result.Period.StartAt.Should().Be(TestData.StartAt);
			result.Period.EndAt.Should().Be(TestData.EndAt);
			result.TotalSeats.Should().Be(TestData.TotalSeats);
			result.AvailableSeats.Should().Be(TestData.TotalSeats);
		}
	}

	/// <summary>
	/// Проверяет поиск несуществующего события по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenEventDoesNotExist_ShouldReturnNull()
	{
		// Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var result = await new EventRepository(context).Find(Guid.NewGuid(), CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Проверяет сохранение события (через цепочку Add + SaveChangesAsync).
	/// </summary>
	[Fact]
	public async Task Add_WhenValidData_ShouldSaveCorrectly()
	{
		// Act
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new EventRepository(context);
			repository.Add(CreateEvent());
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Assert
		await using (var context = DbFixture.CreateContext())
		{
			var result = await context.Events.FindAsync(EventId);

			result.Should().NotBeNull();
			result.Id.Should().Be(EventId);
			result.Title.Should().Be(TestData.Title);
			result.Description.Should().Be(TestData.Description);
			result.Period.StartAt.Should().Be(TestData.StartAt);
			result.Period.EndAt.Should().Be(TestData.EndAt);
			result.TotalSeats.Should().Be(TestData.TotalSeats);
			result.AvailableSeats.Should().Be(TestData.TotalSeats);
		}
	}

	/// <summary>
	/// Проверяет обновление события (через цепочку Find + Update + SaveChangesAsync).
	/// </summary>
	[Fact]
	public async Task Update_WhenValidData_ShouldUpdateCorrectly()
	{
		// Arrange
		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			await context.SaveChangesAsync();
		}

		// Act
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new EventRepository(context);
			var @event = (await repository.Find(EventId, CancellationToken.None))!;
			@event.Update(TestData.UpdatedTitle, TestData.UpdatedDescription,
				EventPeriod.Create(TestData.UpdatedStartAt, TestData.UpdatedEndAt));
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Assert
		await using (var context = DbFixture.CreateContext())
		{
			var result = await context.Events.FindAsync(EventId);

			result.Should().NotBeNull();
			result.Id.Should().Be(EventId);
			result.Title.Should().Be(TestData.UpdatedTitle);
			result.Description.Should().Be(TestData.UpdatedDescription);
			result.Period.StartAt.Should().Be(TestData.UpdatedStartAt);
			result.Period.EndAt.Should().Be(TestData.UpdatedEndAt);
			result.TotalSeats.Should().Be(TestData.TotalSeats);
			result.AvailableSeats.Should().Be(TestData.TotalSeats);
		}
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public async Task Delete_And_SaveChangesAsync_WhenValidData_ShouldDeleteCorrectly()
	{
		// Arrange
		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			await context.SaveChangesAsync();
		}

		// Act
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new EventRepository(context);
			var eventToDelete = await repository.Find(EventId, CancellationToken.None);
			repository.Delete(eventToDelete!);
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Assert
		await using (var context = DbFixture.CreateContext())
		{
			var result = await context.Events.FindAsync(EventId);
			result.Should().BeNull();
		}
	}

	/// <summary>
	/// Проверяет удаление события, на которое есть бронирование.
	/// </summary>
	[Fact]
	public async Task Delete_And_SaveChangesAsync_WhenEventBookingExists_ShouldThrowForeignKeyException()
	{
		// Arrange
		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			context.Bookings.Add(Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow));
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var repository = new EventRepository(context);
			var eventToDelete = await repository.Find(EventId, CancellationToken.None);
			repository.Delete(eventToDelete!);

			//Assert
			Func<Task> act = async () => await repository.SaveChangesAsync(CancellationToken.None);
			await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateException>();
		}
	}

	/// <summary>
	/// Проверяет существование события по идентификатору.
	/// </summary>
	[Fact]
	public async Task Exists_WhenEventExists_ShouldReturnTrue()
	{
		// Arrange
		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			context.Bookings.Add(Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow));
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var repository = new EventRepository(context);
			var result = await repository.Exists(EventId, CancellationToken.None);

			//Assert
			result.Should().BeTrue();
		}
	}

	/// <summary>
	/// Проверяет существование события по идентификатору.
	/// </summary>
	[Fact]
	public async Task Exists_WhenEventDoesNotExist_ShouldReturnFalse()
	{
		//Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var repository = new EventRepository(context);
		var result = await repository.Exists(Guid.NewGuid(), CancellationToken.None);

		//Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Проверяет фильтрацию по наименованию события.
	/// </summary>
	[Theory]
	[InlineData("23")]
	[InlineData("23 февраля")]
	[InlineData("фев")]
	[InlineData("вра")]
	[InlineData("аля")]
	[InlineData("ФеВрАлЯ")]
	public async Task GetFiltered_WhenTitleFilterIsProvided_ShouldFilterByTitle(string title)
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page: 1, pageSize: 10,
			new Filters(Title: title), CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Data.Should().HaveCount(1);
		var item = result.Data.Should().ContainSingle().Subject;
		item.Title.Should().Be("23 февраля");
	}

	/// <summary>
	/// Проверяет фильтрацию по дате начала события.
	/// </summary>
	[Theory]
	[InlineData(0, 2)]
	[InlineData(-6, 3)]
	public async Task GetFiltered_WhenFromFilterIsProvided_ShouldFilterByStartAt(int daysToAdd, int totalItems)
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page: 1, pageSize: 10,
			new Filters(From: Date.AddDays(daysToAdd)), CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Data.Should().OnlyContain(item => item.Period.StartAt >= Date.AddDays(daysToAdd));
	}

	/// <summary>
	/// Проверяет фильтрацию по дате окончания события.
	/// </summary>
	[Theory]
	[InlineData(0, 3)]
	[InlineData(-5, 0)]
	public async Task GetFiltered_WhenToFilterIsProvided_ShouldFilterByEndAt(int monthToAdd, int totalItems)
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page: 1, pageSize: 10,
			new Filters(To: Date.AddMonths(monthToAdd)), CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Data.Should().OnlyContain(item => item.Period.EndAt <= Date.AddMonths(monthToAdd));
	}

	/// <summary>
	/// Проверяет комбинированную фильтрацию.
	/// </summary>
	[Theory]
	[InlineData(6, 1)]
	[InlineData(8, 2)]
	public async Task GetFiltered_WhenCombinedFiltersAreProvided_ShouldFilterCorrectly(int endAtAddDays,
		int filteredItems)
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page: 1, pageSize: 10,
			new Filters(Title: "День", Date.AddDays(-1), Date.AddDays(endAtAddDays)), CancellationToken.None);

		// Assert
		result.Data.Should().HaveCount(filteredItems);
	}

	/// <summary>
	/// Проверяет пагинацию событий.
	/// </summary>
	[Theory]
	[InlineData(1, 3, 3)]
	[InlineData(2, 3, 2)]
	[InlineData(3, 3, 0)]
	[InlineData(1, 2, 2)]
	[InlineData(2, 2, 2)]
	[InlineData(3, 2, 1)]
	public async Task GetFiltered_WhenMultiplePagesExist_ShouldPaginateCorrectly(int page, int pageSize,
		int itemsPerPage)
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page, pageSize, new Filters(),
			CancellationToken.None);

		// Assert
		result.TotalItems.Should().Be(5);
		result.Data.Should().HaveCount(itemsPerPage);
	}

	[Fact]
	public async Task GetFiltered_WhenNoEventsMatchFilters_ShouldReturnEmptyResult()
	{
		// Arrange
		await SeedData();

		// Act
		await using var context = DbFixture.CreateContext();
		var result = await new EventRepository(context).GetFiltered(page: 1, pageSize: 10,
			new Filters(Title: "март"), CancellationToken.None);

		// Assert
		result.TotalItems.Should().Be(0);
		result.Data.Should().BeEmpty();
	}
}