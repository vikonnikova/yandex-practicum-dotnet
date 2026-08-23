using Events.Application.Contracts.Auth;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.UseCases.Auth;

public class RegisterUserCommandHandler(IPasswordHasher hasher, IUserRepository userRepository)
	: IRequestHandler<RegisterUserCommand>
{
	public async Task Handle(RegisterUserCommand command, CancellationToken cancellationToken)
	{
		var passwordHash = hasher.Hash(command.Password);
		var userId = Guid.NewGuid();

		if (await userRepository.ExistsByLogin(command.Login, cancellationToken))
		{
			throw new UserAlreadyExistsException();
		}
		
		userRepository.Add(User.Create(userId, command.Login, passwordHash, command.Role));
		await userRepository.SaveChangesAsync(cancellationToken);
	}
}