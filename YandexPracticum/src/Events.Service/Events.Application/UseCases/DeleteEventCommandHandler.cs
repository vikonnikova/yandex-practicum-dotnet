using Events.Application.Contracts.Commands;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using MediatR;

namespace Events.Application.UseCases;

internal class DeleteEventCommandHandler(IEventRepository repository) : IRequestHandler<DeleteEventCommand>
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
    }
}