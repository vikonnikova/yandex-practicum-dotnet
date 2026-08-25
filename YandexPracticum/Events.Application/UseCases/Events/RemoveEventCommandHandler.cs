using Events.Application.Contracts.Commands.Events;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using MediatR;

namespace Events.Application.UseCases.Events;

internal class RemoveEventCommandHandler(IEventRepository repository) : IRequestHandler<RemoveEventCommand>
{
    public async Task Handle(RemoveEventCommand command, CancellationToken cancellationToken)
    {
        var eventToDelete = await repository.Find(command.EventId, cancellationToken);

        if (eventToDelete is null)
        {
            throw new EntityNotFoundException("Событие", command.EventId);
        }

        repository.Delete(eventToDelete);

        await repository.SaveChangesAsync(cancellationToken);
    }
}