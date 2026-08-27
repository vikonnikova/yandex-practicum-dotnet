using Events.Application.Contracts.Commands;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.UseCases;

internal class CreateEventCommandHandler(IEventRepository repository) : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid();

        repository.Add(Event.Create(eventId, command.Title, command.Description,
            EventPeriod.Create(command.StartAt, command.EndAt), command.TotalSeats));

        await repository.SaveChangesAsync(cancellationToken);

        return eventId;
    }
}