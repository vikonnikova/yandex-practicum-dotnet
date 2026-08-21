using Events.Application.Contracts;
using Events.Application.Interfaces;
using Events.Domain;

namespace Events.Application.UseCases;

public class AddUserCommandHandler(IUserRepository repository)
{
	public async Task Handle(AddUserCommand command, CancellationToken cancellationToken)
	{
		repository.Add(User.Create(Guid.NewGuid(), command.Login, command.PasswordHash, command.Role));
		await repository.SaveChangesAsync(cancellationToken);
	}
}