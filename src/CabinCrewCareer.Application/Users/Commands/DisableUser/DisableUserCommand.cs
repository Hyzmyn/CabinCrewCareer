using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.DisableUser
{
    public record DisableUserCommand(int UserId) : IRequest<Response<string>>;
}
