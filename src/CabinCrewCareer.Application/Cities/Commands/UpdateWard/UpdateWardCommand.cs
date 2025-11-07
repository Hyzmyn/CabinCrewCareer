using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Commands.UpdateWard
{
    public record UpdateWardCommand(int WardId, string WardName) : IRequest<Response<string>>;
}
