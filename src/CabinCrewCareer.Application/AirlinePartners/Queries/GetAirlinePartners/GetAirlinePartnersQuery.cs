using CabinCrewCareer.Application.AirlinePartners.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.AirlinePartners.Queries.GetAirlinePartners
{
    public record GetAirlinePartnersQuery() : IRequest<Response<List<GetAirlinePartnerResponse>>>;
}
