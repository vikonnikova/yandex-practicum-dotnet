using Events.Application.Contracts.Commands;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Settings;
using MediatR;

namespace Events.Application.UseCases;

internal class DeleteEventCommandHandler(
    IEventRepository repository,
    ICacheService cache) : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        var eventToDelete = await repository.Find(command.EventId, cancellationToken);

        if (eventToDelete is null)
        {
            throw new EntityNotFoundException("Событие", command.EventId);
        }

        repository.Delete(eventToDelete);

        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Event(command.EventId), cancellationToken);
    }
}
