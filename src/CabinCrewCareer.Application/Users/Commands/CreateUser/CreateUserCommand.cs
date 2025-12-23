using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(
    string Username,
    string FullName,
    string PhoneNumber,
    string Email,
    DateOnly DateOfBirth,
    int? PartnerId,
    int RoleId) : IRequest<Response<string>>;
}
