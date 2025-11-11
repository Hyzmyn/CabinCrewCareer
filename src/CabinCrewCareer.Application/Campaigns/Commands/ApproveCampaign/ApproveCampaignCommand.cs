using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.ApproveCampaign
{
    public record ApproveCampaignCommand(int CampaignId) : IRequest<Response<bool>>;
}