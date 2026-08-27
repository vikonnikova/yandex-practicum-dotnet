using Events.Api.Contracts;
using Events.Api.Contracts.Bookings;
using Events.Domain;
using BookingStatus = Events.Api.Contracts.Bookings.BookingStatus;

namespace Events.Api.Mappings;

internal static class BookingMapping
{
    internal static BookingResponse ToResponse(this Booking dto)
    {
        return new BookingResponse(dto.Id, dto.EventId, MapStatus(dto.Status));
    }

    private static BookingStatus MapStatus(Events.Domain.BookingStatus value)
    {
        return value switch
        {
            Domain.BookingStatus.Pending => BookingStatus.Pending,
            Domain.BookingStatus.Confirmed => BookingStatus.Confirmed,
            Domain.BookingStatus.Rejected => BookingStatus.Rejected,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}