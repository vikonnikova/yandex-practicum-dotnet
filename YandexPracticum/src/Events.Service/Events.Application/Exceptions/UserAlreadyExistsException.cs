namespace Events.Application.Exceptions;

public class UserAlreadyExistsException() : Exception("Пользователь с таким логином уже существует");