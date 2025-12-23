using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.AirlinePartners.Commands.CreateAirlinePartner
{
    public record CreateAirlinePartnerCommand(
        string PartnerName,
        string Description,
        string Address,
        int WardId) : IRequest<Response<string>>;
}
