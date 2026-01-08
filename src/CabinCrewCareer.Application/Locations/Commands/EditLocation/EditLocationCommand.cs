using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Locations.Commands.EditLocation
{
    public record EditLocationCommand(
        int LocationId,
        EditLocationRequest EditLocationRequest) : IRequest<Response<string>>;
}
