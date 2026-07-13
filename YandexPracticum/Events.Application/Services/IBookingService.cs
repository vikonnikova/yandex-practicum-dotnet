using Events.Application.Services.Dto;

namespace Events.Application.Services;

public interface IBookingService
{
	Task<BookingDto> GetById(Guid bookingId, CancellationToken cancellationToken);

	Task<BookingDto> Add(BookingToAddDto bookingData, CancellationToken cancellationToken);
}