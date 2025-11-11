using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.RejectCampaign
{
    public record RejectCampaignCommand(int CampaignId, string RejectReason) : IRequest<Response<bool>>;
}