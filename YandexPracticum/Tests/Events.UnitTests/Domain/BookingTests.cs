using Events.Domain;
using Events.Domain.Exceptions;
using FluentAssertions;

namespace Events.UnitTests.Domain;

public class BookingTests
{
    /// <summary>
    /// Проверяет создание бронирования.
    /// </summary>
    [Fact]
    public void Create_WhenValidData_ShouldWorkCorrectly()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var booking = Booking.Create(bookingId, eventId, userId, now);

        booking.Id.Should().Be(bookingId);
        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().Be(now);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    /// Проверяет подтверждение бронирования.
    /// </summary>
    [Fact]
    public void Confirm_WhenValidData_ShouldWorkCorrectly()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var confirmedAt = DateTime.UtcNow.AddDays(5).AddHours(4).AddMinutes(8);
        var booking = Booking.Create(bookingId, eventId, userId, now);

        booking.Confirm(confirmedAt);

        booking.Id.Should().Be(bookingId);
        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().Be(now);
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().Be(confirmedAt);
    }

    /// <summary>
    /// Проверяет отклонение бронирования.
    /// </summary>
    [Fact]
    public void Reject_WhenValidData_ShouldWorkCorrectly()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var rejectedAt = DateTime.UtcNow.AddDays(1).AddHours(6).AddMinutes(20);
        var booking = Booking.Create(bookingId, eventId, userId, now);

        booking.Reject(rejectedAt);

        booking.Id.Should().Be(bookingId);
        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().Be(now);
        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().Be(rejectedAt);
    }

    /// <summary>
    /// Проверяет отмену бронирования.
    /// </summary>
    [Fact]
    public void Cancel_WhenValidData_ShouldWorkCorrectly()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var cancelledAt = DateTime.UtcNow.AddDays(1).AddHours(6).AddMinutes(20);
        var booking = Booking.Create(bookingId, eventId, userId, now);

        booking.Cancel(cancelledAt);

        booking.Id.Should().Be(bookingId);
        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().Be(now);
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.ProcessedAt.Should().Be(cancelledAt);
    }

    /// <summary>
    /// Проверяет подтверждение отклоненного бронирования.
    /// </summary>
    [Fact]
    public void Confirm_WhenBookingIsReject_ShouldThrowBookingMustBeInPendingStatusException()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        booking.Reject(DateTime.UtcNow);

        Assert.Throws<BookingMustBeInPendingStatusException>(() => booking.Confirm(DateTime.UtcNow));
    }

    /// <summary>
    /// Проверяет отклонение отмененного бронирования.
    /// </summary>
    [Fact]
    public void Reject_WhenBookingIsCanceled_ShouldThrowBookingMustBeInPendingStatusException()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        booking.Cancel(DateTime.UtcNow);

        Assert.Throws<BookingMustBeInPendingStatusException>(() => booking.Reject(DateTime.UtcNow));
    }

    /// <summary>
    /// Проверяет отмену подтвержденного бронирования.
    /// </summary>
    [Fact]
    public void Cancel_WhenBookingIsConfirmed_ShouldThrowBookingMustBeInPendingStatusException()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        booking.Confirm(DateTime.UtcNow);

        Assert.Throws<BookingMustBeInPendingStatusException>(() => booking.Cancel(DateTime.UtcNow));
    }
}