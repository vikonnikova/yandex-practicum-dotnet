namespace Events.Domain.Exceptions;

public class BookingLimitReachingException(string message) : Exception(message);