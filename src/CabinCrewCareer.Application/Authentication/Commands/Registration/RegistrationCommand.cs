using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Authentication.Commands.Registration
{
    public record RegistrationCommand(
    string Username,
    string Password,
    string ConfirmedPassword,
    string FullName,
    int Gender,
    DateOnly DateOfBirth,
    string PhoneNumber,
    string Email) : IRequest<Response<string>>;
}
