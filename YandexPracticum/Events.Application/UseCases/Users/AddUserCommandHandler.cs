using Events.Application.Contracts.Commands.Users;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.UseCases.Users;

internal class AddUserCommandHandler(IUserRepository repository) : IRequestHandler<AddUserCommand, Guid>
{
	public async Task<Guid> Handle(AddUserCommand command, CancellationToken cancellationToken)
	{
		var userId = Guid.NewGuid();
		repository.Add(User.Create(userId, command.Login, command.PasswordHash, command.Role));
		await repository.SaveChangesAsync(cancellationToken);

		return userId;
	}
}