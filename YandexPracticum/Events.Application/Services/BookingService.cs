using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Mappings;
using Events.Application.UseCases.Dto;
using Events.Domain;

namespace Events.Application.UseCases;

public class BookingService(
	IBookingRepository repository,
	IEventRepository eventRepository,
	IBookingTaskQueue taskQueue) : IBookingService
{
	public BookingDto GetById(Guid bookingId)
	{
		var booking = repository.Find(bookingId);

		return booking?.ToDto() ?? throw new EntityNotFoundException("Бронь", bookingId);
	}

	public BookingDto Add(BookingToAddDto bookingData)
	{
		if (!eventRepository.Exists(bookingData.EventId))
		{
			throw new EntityNotFoundException("Событие", bookingData.EventId);
		}
		
		var booking = Booking.Create(bookingData.Id, bookingData.EventId);
		repository.Add(booking);

		taskQueue.Enqueue(new BookingTask(booking.Id));

		return booking.ToDto();
	}

	public void Confirm(Guid bookingId)
	{
		var booking = repository.Find(bookingId);

		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Confirm();
	}

	public void Reject(Guid bookingId)
	{
		var booking = repository.Find(bookingId);

		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Reject();
	}
}