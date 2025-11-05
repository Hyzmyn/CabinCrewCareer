using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public record CreateCampaignRequestCommand(
        string CampaignName,
        string Description,
        int TargetQuantity) : IRequest<Response<GetCampaignRequestResponse>>;
}
