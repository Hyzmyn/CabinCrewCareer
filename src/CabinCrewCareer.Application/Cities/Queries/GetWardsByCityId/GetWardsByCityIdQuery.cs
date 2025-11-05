using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Queries.GetWardsByCityId
{
    public record GetWardsByCityIdQuery(int CityId) : IRequest<Response<List<GetWardResponse>>>;
}
