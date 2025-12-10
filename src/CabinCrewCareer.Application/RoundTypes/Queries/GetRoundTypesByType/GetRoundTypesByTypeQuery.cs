using CabinCrewCareer.Application.RoundTypes.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Queries.GetRoundTypesByType
{
    public record GetRoundTypesByTypeQuery(CampaignType CampaignType) : IRequest<Response<List<GetRoundTypeResponse>>>;
}
