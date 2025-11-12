using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRounds.Queries.GetCampaignRoundById
{
    public record GetCampaignRoundByIdQuery(int CampaignRoundId) : IRequest<Response<GetCampaignRoundResponse>>;
}
