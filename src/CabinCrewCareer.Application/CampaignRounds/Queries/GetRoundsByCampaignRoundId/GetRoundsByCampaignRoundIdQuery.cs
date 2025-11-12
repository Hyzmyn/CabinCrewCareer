using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRounds.Queries.GetRoundsByCampaignRoundId
{
    public record GetRoundsByCampaignRoundIdQuery(int CampaignRoundId) : IRequest<Response<List<GetRoundResponse>>>;
}
