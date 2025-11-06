using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateRejectedCampaignRequest
{
    public record UpdateRejectedCampaignRequestCommand(
        int RequestId,
        string CampaignName,
        string Description,
        int TargetQuantity) : IRequest<Response<GetCampaignRequestResponse>>;
}
