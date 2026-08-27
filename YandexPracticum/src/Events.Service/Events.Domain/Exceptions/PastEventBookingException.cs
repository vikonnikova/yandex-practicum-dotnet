namespace Events.Domain.Exceptions;

public class PastEventBookingException() : Exception("Попытка забронировать прошедшее событие.");