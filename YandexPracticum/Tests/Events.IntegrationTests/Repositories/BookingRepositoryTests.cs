using Events.Domain;
using Events.Infrastructure;
using Events.IntegrationTests.Repositories.Base;
using FluentAssertions;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class BookingRepositoryTests(DbFixture dbFixture) : BaseRepositoryTest(dbFixture)
{
    /// <summary>
    /// Проверяет поиск брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenValidData_ShouldReturnBooking()
    {
        // Arrange
        var createdAt = new DateTime(2022, 04, 04, 12, 00, 00, DateTimeKind.Utc);
        var booking = Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId, createdAt);

        await using (var context = DbFixture.CreateContext())
        {
            context.Users.Add(TestData.TestUser);
            context.Events.Add(TestData.TestEvent);
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        await using (var context = DbFixture.CreateContext())
        {
            // Act
            var result = await new BookingRepository(context).Find(TestData.BookingId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(TestData.BookingId);
            result.EventId.Should().Be(TestData.EventId);
            result.CreatedAt.Should().Be(createdAt);
            result.Status.Should().Be(BookingStatus.Pending);
        }
    }

    /// <summary>
    /// Проверяет поиск несуществующей брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenBookingDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context = DbFixture.CreateContext();

        // Act
        var result = await new BookingRepository(context).Find(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет получение необработанных броней.
    /// </summary>
    [Fact]
    public async Task GetPending_WhenDifferentStatuses_ShouldReturnOnlyPendingBookings()
    {
        // Arrange
        var pendingBooking1 = Booking.Create(Guid.NewGuid(), TestData.EventId, TestData.UserId, DateTime.UtcNow);
        var pendingBooking2 = Booking.Create(Guid.NewGuid(), TestData.EventId, TestData.UserId, DateTime.UtcNow);
        var confirmedBooking = Booking.Create(Guid.NewGuid(), TestData.EventId, TestData.UserId, DateTime.UtcNow);
        var rejectedBooking = Booking.Create(Guid.NewGuid(), TestData.EventId, TestData.UserId, DateTime.UtcNow);

        confirmedBooking.Confirm(DateTime.UtcNow);
        rejectedBooking.Reject(DateTime.UtcNow);

        await using (var context = DbFixture.CreateContext())
        {
            context.Users.Add(TestData.TestUser);
            context.Events.Add(TestData.TestEvent);
            context.Bookings.AddRange(pendingBooking1, pendingBooking2, confirmedBooking, rejectedBooking);
            await context.SaveChangesAsync();
        }

        await using (var context = DbFixture.CreateContext())
        {
            // Act
            var result = await new BookingRepository(context).GetPending(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain([pendingBooking1.Id, pendingBooking2.Id]);
            result.Should().NotContain([confirmedBooking.Id, rejectedBooking.Id]);
        }
    }

    /// <summary>
    /// Проверяет сохранение брони (через цепочку Add + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Add_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        var createdAt = new DateTime(2022, 04, 04, 12, 00, 00, DateTimeKind.Utc);
        await using (var context = DbFixture.CreateContext())
        {
            context.Users.Add(TestData.TestUser);
            context.Events.Add(TestData.TestEvent);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            repository.Add(Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId, createdAt));
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(TestData.BookingId);

            result.Should().NotBeNull();
            result.Id.Should().Be(TestData.BookingId);
            result.EventId.Should().Be(TestData.EventId);
            result.CreatedAt.Should().Be(createdAt);
            result.Status.Should().Be(BookingStatus.Pending);
            result.ProcessedAt.Should().BeNull();
        }
    }

    /// <summary>
    /// Проверяет подтверждение заявки на бронь (через цепочку Find + Confirm + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Confirm_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        var createdAt = new DateTime(2022, 04, 04, 12, 00, 00, DateTimeKind.Utc);
        var processedAt = new DateTime(2022, 04, 05, 15, 37, 00, DateTimeKind.Utc);
        await using (var context = DbFixture.CreateContext())
        {
            context.Users.Add(TestData.TestUser);
            context.Events.Add(TestData.TestEvent);
            context.Bookings.Add(Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId, createdAt));
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            var booking = (await repository.Find(TestData.BookingId, CancellationToken.None))!;
            booking.Confirm(processedAt);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(TestData.BookingId);

            result.Should().NotBeNull();
            result.Id.Should().Be(TestData.BookingId);
            result.EventId.Should().Be(TestData.EventId);
            result.CreatedAt.Should().Be(createdAt);
            result.Status.Should().Be(BookingStatus.Confirmed);
            result.ProcessedAt.Should().Be(processedAt);
        }
    }

    /// <summary>
    /// Проверяет отклонение заявки на бронь (через цепочку Find + Reject + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Reject_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        var createdAt = new DateTime(2022, 04, 04, 12, 00, 00, DateTimeKind.Utc);
        var processedAt = new DateTime(2022, 04, 05, 15, 37, 00, DateTimeKind.Utc);
        await using (var context = DbFixture.CreateContext())
        {
            context.Users.Add(TestData.TestUser);
            context.Events.Add(TestData.TestEvent);
            context.Bookings.Add(Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId, createdAt));
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            var booking = (await repository.Find(TestData.BookingId, CancellationToken.None))!;
            booking.Reject(processedAt);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(TestData.BookingId);

            result.Should().NotBeNull();
            result.Id.Should().Be(TestData.BookingId);
            result.EventId.Should().Be(TestData.EventId);
            result.CreatedAt.Should().Be(createdAt);
            result.Status.Should().Be(BookingStatus.Rejected);
            result.ProcessedAt.Should().Be(processedAt);
        }
    }
}