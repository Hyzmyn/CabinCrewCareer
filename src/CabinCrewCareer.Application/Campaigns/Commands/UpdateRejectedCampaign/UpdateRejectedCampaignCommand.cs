using CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateRejectedCampaign
{
    public record UpdateRejectedCampaignCommand(
        int CampaignId,
        DateTime? StartDate,
        DateTime? EndDate,
        List<RoundDto>? Rounds) : IRequest<Response<bool>>;
}