using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.CreateRoundType
{
    public record CreateRoundTypeCommand(string RoundTypeName, CampaignType CampaignType) : IRequest<Response<string>>;
}
