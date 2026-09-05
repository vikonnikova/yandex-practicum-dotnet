using Bookings.Api.Contracts;
using Bookings.Application.Contracts.Queries;
using Bookings.Domain;
using Shared.Contracts;
using BookingStatus = Bookings.Api.Contracts.BookingStatus;

namespace Bookings.Api.Mapping;

internal static class BookingMapping
{
    internal static BookingResponse ToResponse(this Booking dto)
    {
        return new BookingResponse(dto.Id, dto.EventId, MapStatus(dto.Status));
    }

    internal static GetBookingsByUserQuery ToQuery(this GetBookingsQuery data)
    {
        return new GetBookingsByUserQuery(data.Page, data.PageSize);
    }

    internal static PaginatedResult<BookingResponse> ToResponse(this PaginatedResult<Booking> paginatedBookings)
    {
        var data = paginatedBookings.Data.Select(x => new BookingResponse(x.Id, x.EventId, MapStatus(x.Status)))
            .ToArray();
        
        return new PaginatedResult<BookingResponse>(data, paginatedBookings.TotalItems);
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