namespace Bookings.Application.Exceptions;

public class AccessDeniedException(string message) : Exception(message);