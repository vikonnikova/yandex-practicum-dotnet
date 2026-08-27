using Events.Application.Contracts.Commands.Events;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.UseCases.Events;

internal class AddEventCommandHandler(IEventRepository repository) : IRequestHandler<AddEventCommand, Guid>
{
    public async Task<Guid> Handle(AddEventCommand command, CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid();

        repository.Add(Event.Create(eventId, command.Title, command.Description,
            EventPeriod.Create(command.StartAt, command.EndAt), command.TotalSeats));

        await repository.SaveChangesAsync(cancellationToken);

        return eventId;
    }
}