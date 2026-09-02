using Shared.Contracts;

namespace Bookings.Application.Interfaces;

public interface IKafkaPublisher
{
    Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken);
}
