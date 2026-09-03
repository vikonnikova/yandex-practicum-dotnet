using Bookings.Api.Contracts.Bookings;
using Bookings.Application.Contracts.Commands;
using Bookings.Domain;
using BookingStatus = Bookings.Api.Contracts.Bookings.BookingStatus;

namespace Bookings.Api.Mapping;

internal static class BookingMapping
{
    internal static BookingResponse ToResponse(this Booking dto)
    {
        return new BookingResponse(dto.Id, dto.EventId, MapStatus(dto.Status));
    }

    private static BookingStatus MapStatus(Domain.BookingStatus value)
    {
        return value switch
        {
            Domain.BookingStatus.Pending => BookingStatus.Pending,
            Domain.BookingStatus.Confirmed => BookingStatus.Confirmed,
            Domain.BookingStatus.Rejected => BookingStatus.Rejected,
            Domain.BookingStatus.Cancelled => BookingStatus.Cancelled,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}