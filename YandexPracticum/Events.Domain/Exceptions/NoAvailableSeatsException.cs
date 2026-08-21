namespace Events.Domain.Exceptions;

public class NoAvailableSeatsException() : Exception("Нет доступных мест для бронирования на запрашиваемое событие.");