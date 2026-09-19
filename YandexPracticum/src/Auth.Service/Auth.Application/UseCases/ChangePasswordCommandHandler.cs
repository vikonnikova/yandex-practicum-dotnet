using Auth.Application.Contracts.Auth;
using Auth.Application.Exceptions;
using Auth.Application.Interfaces;
using MediatR;

namespace Auth.Application.UseCases;

public class ChangePasswordCommandHandler(
    IPasswordHasher hasher,
    ICurrentUserContext userContext,
    IUserRepository userRepository)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.Find(userContext.UserId, cancellationToken)
                   ?? throw new EntityNotFoundException("Пользователь", userContext.UserId);

        if (!hasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            throw new WrongCurrentPasswordException();
        }

        user.UpdatePassword(hasher.Hash(command.NewPassword));
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}