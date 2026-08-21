namespace Events.Domain.Exceptions;

public class BookingLimitReachingException() : Exception("Достигнут лимит бронирования у события.");