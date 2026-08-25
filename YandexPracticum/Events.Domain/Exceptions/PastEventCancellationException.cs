namespace Events.Domain.Exceptions;

public class PastEventCancellationException() : Exception("Попытка отменить прошедшее событие");