using Events.Application.Contracts.Queries.Users;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers.Users;

public class GetUserByIdQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, User>
{
    public async Task<User> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.Find(query.UserId, cancellationToken);

        return user ?? throw new EntityNotFoundException("Пользователь", query.UserId);
    }
}