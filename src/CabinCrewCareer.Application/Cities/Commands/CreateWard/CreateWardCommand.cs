using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Commands.CreateWard
{
    public record CreateWardCommand(string WardName, int CityId) : IRequest<Response<string>>;
}
