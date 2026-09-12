using Events.Application.Contracts.Commands;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Domain;
using MediatR;

namespace Events.Application.UseCases;

internal class UpdateEventCommandHandler(
    IEventRepository repository,
    ICacheService cache) : IRequestHandler<UpdateEventCommand>
{
    public async Task Handle(UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var eventToUpdate = await repository.Find(command.Id, cancellationToken);

        if (eventToUpdate is null)
        {
            throw new EntityNotFoundException("Событие", command.Id);
        }

        eventToUpdate.Update(command.Title, command.Description,
            EventPeriod.Create(command.StartAt, command.EndAt));

        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.Event(command.Id), cancellationToken);
    }
}