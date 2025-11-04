using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.UpdateUser
{
    public record UpdateUserCommand(
       int Id,
       UpdateUserRequest UpdateUserRequest) : IRequest<Response<GetUserResponse>>;
}
