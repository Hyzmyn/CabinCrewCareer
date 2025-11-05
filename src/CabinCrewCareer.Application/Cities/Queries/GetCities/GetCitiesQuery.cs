using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Cities.Queries.GetCities
{
    public record GetCitiesQuery() : IRequest<Response<List<GetCityResponse>>>;
}
