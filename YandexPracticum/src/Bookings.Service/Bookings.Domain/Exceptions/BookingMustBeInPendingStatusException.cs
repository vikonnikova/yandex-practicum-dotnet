namespace Bookings.Domain.Exceptions;

public class BookingHasWrongStatusException(string message) : Exception(message);