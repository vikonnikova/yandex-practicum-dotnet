using Bookings.Domain.Exceptions;

namespace Bookings.Domain;

public class Booking
{
    private Booking(Guid id, Guid eventId, Guid userId, DateTime createdAt)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        CreatedAt = createdAt;
        Status = BookingStatus.Pending;
    }

    private Booking()
    {
    }

    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public BookingStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public static Booking Create(Guid id, Guid eventId, Guid userId, DateTime createdAt)
    {
        return new Booking(id, eventId, userId, createdAt);
    }

    public void Confirm(DateTime processedAt)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingMustBeInPendingStatusException("Нельзя подтвердить обработанное системой бронирование.");
        }

        Status = BookingStatus.Confirmed;
        ProcessedAt = processedAt;
    }

    public void Reject(DateTime processedAt)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingMustBeInPendingStatusException("Нельзя отклонить обработанное системой бронирование.");
        }

        Status = BookingStatus.Rejected;
        ProcessedAt = processedAt;
    }

    public void Cancel(DateTime processedAt)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
        {
            throw new BookingMustBeInPendingStatusException("Нельзя отменить бронирование.");
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = processedAt;
    }
}