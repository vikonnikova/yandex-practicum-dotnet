using Events.Application.UseCases.Dto;

namespace Events.Application.UseCases;

public interface IBookingService
{
	BookingDto GetById(Guid bookingId);

	BookingDto Add(BookingToAddDto bookingData);
}