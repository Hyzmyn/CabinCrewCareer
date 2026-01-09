using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Locations.Queries.GetLocationById
{
    public record GetLocationByIdQuery(int LocationId) : IRequest<Response<GetLocationDetailResponse>>;
}
