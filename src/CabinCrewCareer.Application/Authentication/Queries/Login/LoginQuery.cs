using CabinCrewCareer.Application.Authentication.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Authentication.Queries.Login
{
    public record LoginQuery(string Username, string Password) : IRequest<Response<AuthenticationResult>>;
}
