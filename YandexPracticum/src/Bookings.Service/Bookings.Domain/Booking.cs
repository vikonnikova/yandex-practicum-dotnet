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
            throw new BookingMustBeInPendingStatusException("Нельзя изменить бронирование. Бронирование подтверждено.");
        }

        Status = BookingStatus.Confirmed;
        ProcessedAt = processedAt;
    }

    public void Reject(DateTime processedAt)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingMustBeInPendingStatusException("Нельзя изменить бронирование. Бронирование отклонено.");
        }

        Status = BookingStatus.Rejected;
        ProcessedAt = processedAt;
    }

    public void Cancel(DateTime processedAt)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingMustBeInPendingStatusException("Нельзя изменить бронирование. Бронирование отменено.");
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = processedAt;
    }
}