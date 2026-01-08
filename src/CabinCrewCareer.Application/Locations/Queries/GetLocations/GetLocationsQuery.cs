using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Locations.Queries.GetLocations
{
    public record GetLocationsQuery() : IRequest<Response<List<GetLocationResponse>>>;
}
