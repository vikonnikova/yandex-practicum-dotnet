using Events.Application.UseCases.Dto;

namespace Events.Application.UseCases;

public interface IBookingService
{
	Task<BookingDto> GetById(Guid bookingId, CancellationToken cancellationToken);

	Task<BookingDto> Add(BookingToAddDto bookingData, CancellationToken cancellationToken);
}