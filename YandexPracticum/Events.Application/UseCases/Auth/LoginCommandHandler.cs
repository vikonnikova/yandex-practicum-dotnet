using Events.Application.Contracts.Auth;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using MediatR;

namespace Events.Application.UseCases.Auth;

public class LoginCommandHandler(IPasswordHasher hasher, IJwtProvider jwtProvider, IUserRepository userRepository)
	: IRequestHandler<LoginCommand, string>
{
	public async Task<string> Handle(LoginCommand command, CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByLogin(command.Login, cancellationToken);

		if (user is null)
		{
			throw new AuthenticationException();
		}

		return hasher.Verify(command.Password, user.PasswordHash)
			? jwtProvider.GenerateToken(user)
			: throw new AuthenticationException();
	}
}