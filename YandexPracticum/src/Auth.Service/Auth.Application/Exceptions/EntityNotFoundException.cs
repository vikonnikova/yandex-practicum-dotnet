namespace Auth.Application.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, int entityId)
        : base($"Сущность [{entityName}] с идентификатором [{entityId}] не найдена.")
    {
    }

    public EntityNotFoundException(string entityName, Guid entityId)
        : base($"Сущность [{entityName}] с идентификатором [{entityId.ToString()}] не найдена.")
    {
    }
}